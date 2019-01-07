namespace smartHookah.Services.Place
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using smartHookah.Controllers;
    using smartHookah.Models;
    using smartHookah.Models.Dto;
    using smartHookah.Support;

    using ReservationDto = smartHookah.Controllers.ReservationDto;

    public class ReservationService: IReservationService
    {
        private readonly SmartHookahContext db;
        private readonly TimeSpan slotDuration = new TimeSpan(0, 30, 0);
        private IEmailService emailService;

        private readonly IPlaceService placeService;

        public ReservationService(SmartHookahContext db, IEmailService emailService, IPlaceService placeService)
        {
            this.db = db;
            this.emailService = emailService;
            this.placeService = placeService;
        }


        public async Task<ReservationManageDto> GetReservationManage(int id, DateTime date)
        {
            var place = await this.placeService.GetManagedPlace(id);
            var tables = SeatDto.FromModelList(place.Seats);
            var reservations = this.TodayReservation(date, place, out var today);
            var placeTime = place.BusinessHours.FirstOrDefault(a => a.Day == date.Day);
            var endTime = date.Date + placeTime.CloseTime;
            if (placeTime.CloseTime < placeTime.OpenTine)
            {
                endTime = endTime + new TimeSpan(1, 0, 0, 0);
            }
            return new ReservationManageDto
                       {
                           Date = date,
                           Tables = tables.ToList(),
                           startTime = date.Date + placeTime.OpenTine,
                           endTime = endTime,
                           Reservations =
                               smartHookah.Models.Dto.ReservationDto.FromModelList(reservations)
                                   .ToList(),
                           TimeSlotSize = 30,
                       };

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