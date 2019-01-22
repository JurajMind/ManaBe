namespace smartHookah.Services.Place
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Threading.Tasks;

    using smartHookah.Models;
    using smartHookah.Models.Dto;
    using smartHookah.Models.Dto.Reservations;
    using smartHookah.Services.Person;
    using smartHookah.Services.Redis;
    using smartHookah.Support;

    public class ReservationService : IReservationService
    {
        private readonly SmartHookahContext db;
        private readonly TimeSpan slotDuration = new TimeSpan(0, 30, 0);
        private IEmailService emailService;

        private readonly IRedisService redisService;

        private readonly IPersonService personService;

        private readonly IPlaceService placeService;

        public ReservationService(SmartHookahContext db, IEmailService emailService, IPlaceService placeService, IPersonService personService, IRedisService redisService)
        {
            this.db = db;
            this.emailService = emailService;
            this.placeService = placeService;
            this.personService = personService;
            this.redisService = redisService;
        }

        public async Task<ReservationManageDto> GetReservationManage(int id, DateTime date)
        {
            var place = await this.placeService.GetManagedPlace(id);
            var tables = SeatDto.FromModelList(place.Seats);
            var reservations = this.TodayReservation(date, place, out var today);
            var placeTime = place.BusinessHours.First(a => a.Day == (int)date.DayOfWeek);
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
                               ReservationDto.FromModelList(reservations)
                                   .ToList(),
                           TimeSlotSize = 30,
                       };
        }

        public async Task<bool> CreateReservation(ReservationDto reservation)
        {
            reservation.PersonId = this.personService.GetCurentPerson().Id;
            var modelReservation = ReservationDto.ToModel(reservation);
            var check = await this.ReservationCheck(modelReservation, reservation.Seats);
            if (!check) return false;
            this.db.Reservations.Add(modelReservation);
            await this.db.SaveChangesAsync();
            return true;
        }

        public async Task<ReservationUsageDto> GetReservationUsage(int placeId, DateTime date)
        {
            var result = this.redisService.GetReservationUsage(placeId, date) ?? await this.UpdateReservationUsage(placeId, date);
            return result;
        }

        public async Task<ReservationUsageDto> UpdateReservationUsage(int placeId, DateTime date)
        {
            var place = await this.placeService.GetPlace(placeId);
            var reservations = this.db.Reservations.Where(a => a.PlaceId == place.Id && EntityFunctions.TruncateTime(a.Time) == date);
            reservations = this.GetActiveReservations(reservations);
            var reservationsList = await reservations.ToListAsync();

            var timesSlots = this.GetTimeSlots(
                date,
                true,
                place.BusinessHours.First(a => a.Day == (int)date.DayOfWeek));

            var tableData = this.CreateTableTable(place.Seats, reservations.ToList(), timesSlots.ToList(), true);

            var reservationUsage = new ReservationUsageDto { TimeSlots = tableData };

            this.redisService.SetReservationUsage(placeId, date, reservationUsage);

            return reservationUsage;
        }

        private async Task<bool> ReservationTableCheck(Reservation reservation, int seatId)
        {
            var seat = await this.db.Seats.Where(s => s.Id == seatId).Include(r => r.Reservations).SingleAsync();
            var reservationDate = reservation.Time.Date;
          
            var posibleConflict = seat?.Reservations.Where(a => a.Time.Date == reservationDate &&
                                                             a.Status != ReservationState.Canceled
                                                             && a.Status != ReservationState.Denied
                                                             && a.Status != ReservationState.NonVisited).ToList();
            return true;
        }

        private async Task<bool> ReservationCheck(Reservation reservation, List<int> seats)
        {
            var place = await this.db.Places.FindAsync(reservation.PlaceId);
            if (place == null)
            {
                return false;
            }

            var seatCheck = seats.Select(s => this.ReservationTableCheck(reservation, s)).ToList();

            await Task.WhenAll(seatCheck);

            foreach (var task in seatCheck)
            {
                if (!await task) return false;
            }

            return true;
        }

        private IEnumerable<Reservation> TodayReservation(DateTime date, Place place, out List<Reservation> todayActiveReservation)
        {
            var todayReservation = this.db.Reservations
                .Where(a => a.PlaceId == place.Id && DbFunctions.TruncateTime(a.Time) == date).ToList();

            todayActiveReservation = todayReservation.Where(
                a => a.Status != ReservationState.Canceled && a.Status != ReservationState.Denied
                     && a.Status != ReservationState.NonVisited).ToList();
            return todayReservation;
        }

        private IQueryable<Reservation> GetActiveReservations(IQueryable<Reservation> reservations)
        {
            return reservations.Where(
                a => a.Status != ReservationState.Canceled && a.Status != ReservationState.Denied
                     && a.Status != ReservationState.NonVisited);
        }
 
        private IEnumerable<TimeSlot> GetTimeSlots(DateTime date, bool includeReservation, BusinessHours placeTime)
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
            ICollection<Seat> seats,
            ICollection<Reservation> reservations,
            ICollection<TimeSlot> timeSlot,
            bool include)
        {
            var result = new List<TimeSlot>();
            var placeCapacity = seats.Sum(s => s.Capacity);

            foreach (var slot in timeSlot)
            {
                var reservation = reservations.Where(
                        r => r.Time.ToShortInt() < slot.Value + this.slotDuration.ToShortInt() && slot.Value < r.Time.ToShortInt() + r.Duration.ToShortInt())
                    .ToList();
                
                var takenTable = reservation.SelectMany(s => s.Seats).Select(s => s.Id);
                var freeTable = seats.Where(s => !takenTable.Contains(s.Id));

                slot.MaxTable = freeTable.Max(s => s.Capacity);
                slot.CapacityLeft = placeCapacity - reservation.EmptyIfNull().Sum(s => s.Persons);
                slot.Reserved = slot.CapacityLeft == 0;
                result.Add(slot);
            }

            return result;
        }
    }

    public class ReservationUsageDto
    {
        public  List<TimeSlot> TimeSlots { get; set; }
    }

    public class ReservationDataDto
    {
        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int SlotCount { get; set; }

        public int MinimalReservation { get; set; }
    }
}