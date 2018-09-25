namespace smartHookah.Services.Place
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;

    using smartHookah.Controllers;
    using smartHookah.Models;
    using smartHookah.Support;

    public class ReservationService: IReservationService
    {
        private readonly SmartHookahContext db;
        private readonly TimeSpan slotDuration = new TimeSpan(0, 30, 0);
        private IEmailService emailService;

        public ReservationService(SmartHookahContext db, IEmailService emailService)
        {
            this.db = db;
            this.emailService = emailService;
        }

        public ReservationInfo GetReservation(int id, DateTime date, bool includeReservation = false)
        {
            var place = db.Places.Where(a => a.Id == id).Include(a => a.Seats).FirstOrDefault();
            
            var todayDay = (int)date.DayOfWeek;
            var timeSlot = new List<TimeSlot>();

            var placeTime = place.BusinessHours.FirstOrDefault(a => a.Day == todayDay);
            var model = new ReservationInfo();
            if (placeTime != null)
            {
                timeSlot = this.GetTimeSlots(date, includeReservation, placeTime);

                List<Reservation> todayActiveReservation;
                var todayReservation = this.TodayReservation(date, place, out todayActiveReservation);

                model.Times = new List<KeyValuePair<int, string>>();

                var minReservationTime = place.MinimumReservationTime;
                model.MinimumReservationTime = place.MinimumReservationTime;
                if (includeReservation)
                {
                    minReservationTime = 1;
                }

                for (int i = minReservationTime; i < 9; i++)
                {
                    var text = $"{ i * 0.5}";
                    model.Times.Add(new KeyValuePair<int, string>(i, text));
                }

                if (includeReservation)
                {
                    model.Times.Add(new KeyValuePair<int, string>(timeSlot.Count, "Do zavíračky"));
                }


                model.Tables = new List<TableDto>(place.Seats
                    .Select(a => new TableDto { Id = a.Id, Capacity = a.Capacity, Name = a.Name }).ToList());

                if (includeReservation)
                {
                    model.TodayReservation = todayActiveReservation.ToDictionary(
                        a => a.Id.ToString(),
                        a => new ReservationDto(a, this.slotDuration));
                    model.Canceled = todayReservation
                        .Where(a => a.Status == ReservationState.Canceled || a.Status == ReservationState.Denied || a.Status == ReservationState.NonVisit)
                        .Select(a => new ReservationDto(a, this.slotDuration));
                }


                model.Reservations = new Dictionary<string, List<TimeSlot>>();

                foreach (var table in model.Tables)
                {
                    var tableReservation = todayActiveReservation.Where(a => a.Seats.Any(b => b.Id == table.Id));

                    var timeTable = this.CreateTableTable(tableReservation, timeSlot, includeReservation);
                    model.Reservations.Add(table.Id.ToString(), timeTable);
                }

                foreach (var slot in timeSlot)
                {
                    var slotData = model.Reservations.Values.SelectMany(a => a.ToArray()).Where(a => a.Value == slot.Value);

                    if (slotData.Count(a => !a.Reserved) <= 0)
                        slot.Reserved = true;
                }


                model.TimeSlots = timeSlot;
            }
            return model;
        }

        private List<Reservation> TodayReservation(DateTime date, Place place, out List<Reservation> todayActiveReservation)
        {
            var todayReservation = this.db.Reservations
                .Where(a => a.PlaceId == place.Id && DbFunctions.TruncateTime(a.Time) == date).ToList();

            todayActiveReservation = todayReservation.Where(
                a => a.Status != ReservationState.Canceled && a.Status != ReservationState.Denied
                     && a.Status != ReservationState.NonVisit).ToList();
            return todayReservation;
        }

 
        private List<TimeSlot> GetTimeSlots(DateTime date, bool includeReservation, BusinessHours placeTime )
        {
            var result = new List<TimeSlot>();
            var startTime = placeTime.OpenTine;
            var nowTime = DateTime.Now.RoundUp(new TimeSpan(0, 30, 0)).TimeOfDay;
            if (date.Date == DateTime.Now.Date && !includeReservation)
                startTime = new TimeSpan(Math.Max(nowTime.Ticks, placeTime.OpenTine.Ticks));

            var endTime = placeTime.CloseTime;

            // If close time pass over midnight , add whole day to end time 
            if (endTime < startTime)
            {
                endTime += new TimeSpan(24, 0, 0);
            }
            var index = 0;
            while (startTime <= endTime)
            {
                result.Add(
                    new TimeSlot { Value = startTime.ToShortInt(), Text = startTime.ToString(@"hh\:mm"), OrderIndex = index });
                index++;
                startTime = startTime + this.slotDuration;
            }
            return result;
        }

        private List<TimeSlot> CreateTableTable(
            IEnumerable<Reservation> tableReservation,
            IList<TimeSlot> timeSlot,
            bool include)
        {
            var takenTime = tableReservation.SelectMany(a => this.TakenTime(a, this.slotDuration, include));

            var freeTime = timeSlot.Where(a => takenTime.All(y => y.Value != a.Value)).ToList();

            return takenTime.Union(freeTime).Distinct(new Compare()).OrderBy(a => a.Value).ToList();
        }

        private IEnumerable<TimeSlot> TakenTime(Reservation reservation, TimeSpan slotDuration, bool include)
        {
            var startTime = reservation.Time.ToShortInt();
            var reservationId = 0;
            if (include)
                reservationId = reservation.Id;
            yield return new TimeSlot
                             {
                                 Value = startTime,
                                 Reserved = true,
                                 Id = reservationId
                             };
            var TimeLeft = reservation.Duration - slotDuration;
            var index = 1;
            while (TimeLeft > TimeSpan.Zero)
            {
                var time = reservation.Time + slotDuration.Multiply(index);
                index++;
                TimeLeft = TimeLeft - slotDuration;
                yield return new TimeSlot
                                 {
                                     Value = time.ToShortInt(),
                                     Reserved = true,
                                     Id = -1
                                 };
            }
        }
    }
}