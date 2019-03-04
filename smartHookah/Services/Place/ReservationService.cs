using System.Data.Entity.Migrations;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Reservations;
using smartHookah.Services.Person;
using smartHookah.Services.Redis;
using smartHookah.Support;
using ServiceStack.Common.Extensions;

namespace smartHookah.Services.Place
{


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
            var place = await placeService.GetManagedPlace(id);
            var tables = SeatDto.FromModelList(place.Seats);
            var reservations = TodayReservation(date, place, out var today);
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

        public async Task<ReservationDto> CreateReservation(ReservationDto reservation)
        {
            reservation.PersonId = personService.GetCurentPerson().Id;
            reservation.Created = DateTime.UtcNow;
            reservation.Started = null;
            var modelReservation = ReservationDto.ToModel(reservation);
            modelReservation.Status = ReservationState.ConfirmationRequired;


            var check = await AllocateSeat(modelReservation);
            if (check != null)
            {
                var seat = await this.db.Seats.FindAsync(check.Value);
                modelReservation.Seats.Add(seat);
                modelReservation.Status = ReservationState.Confirmed;
            }

            db.Reservations.Add(modelReservation);
            await db.SaveChangesAsync();
            var dbReservation = await this.db.Reservations.FindAsync(modelReservation.Id);
            return ReservationDto.FromModel(dbReservation);
        }

        public async Task<ReservationUsage> GetReservationUsage(int placeId, DateTime date)
        {
            //var result = this.redisService.GetReservationUsage(placeId, date) ?? await this.UpdateReservationUsage(placeId, date);
            var result = await UpdateReservationUsage(placeId, date);
            return result;
        }

        public async Task<ReservationUsage> UpdateReservationUsage(int placeId, DateTime date)
        {
            var place = await placeService.GetPlace(placeId);

            if (place.BusinessHours.FirstOrDefault(a => a.Day == (int) date.DayOfWeek) == null)
            {
                return new ReservationUsage();
            } 

            var reservations = db.Reservations.Where(a => a.PlaceId == place.Id && EntityFunctions.TruncateTime(a.Time) == date);
            reservations = GetActiveReservations(reservations);
            var reservationsList = await reservations.ToListAsync();

            var timesSlots = GetTimeSlots(
                date,
                place.BusinessHours.First(a => a.Day == (int)date.DayOfWeek));

            var tableData = CreateTableTable(place.Seats, reservations.ToList(), timesSlots.ToList(), true);

            var reservationUsage = new ReservationUsage { TimeSlots = tableData };

            redisService.SetReservationUsage(placeId, date, reservationUsage);

            return reservationUsage;
        }

        public ICollection<Reservation> GetPersonReservations()
        {
            var person = personService.GetCurentPerson();
            var now = DateTime.UtcNow;
            return person.Reservations.Where(a => a.Time.Add(a.Duration) > now).ToList();
        }

        private async Task<bool> ReservationTableCheck(Reservation reservation, int seatId)
        {
            var seat = await db.Seats.Where(s => s.Id == seatId).Include(r => r.Reservations).SingleAsync();
            var reservationDate = reservation.Time.Date;
          
            var posibleConflict = seat?.Reservations.Where(a => a.Time.Date == reservationDate &&
                                                             a.Status != ReservationState.Canceled
                                                             && a.Status != ReservationState.Denied
                                                             && a.Status != ReservationState.NonVisited).ToList();
            return true;
        }

        private IEnumerable<int> getReservationTimeSlots(Reservation reservation, BusinessHours businessHours)
        {
            var timeSlot = this.GetTimeSlots(reservation.Time.Date, businessHours);
            var reservationStart = reservation.Time.TimeOfDay.ToShortInt();
            var slots = timeSlot.SkipWhile(s => s.Value + slotDuration.ToShortInt() < reservationStart);
            var takenSlots = Math.Round(reservation.Duration.Ticks / (double)slotDuration.Ticks);
            return slots.Take((int)takenSlots).Select(s => s.Value);
        }

        private async Task<int?> AllocateSeat(Reservation reservation)
        {
           
            var place = await db.Places.FindAsync(reservation.PlaceId);
            if (place == null)
            {
                return null;
            }

            var tableUsage = await this.GetReservationUsage(place.Id, reservation.Time.Date);

            var reservationSlots = getReservationTimeSlots(reservation,place.BusinessHours.FirstOrDefault(s => s.Day == (int) reservation.Time.DayOfWeek)).ToList();

            if (reservationSlots.Count() > (int) Math.Round(reservation.Duration.Ticks / (double) slotDuration.Ticks))
            {
                return null;
            }

            var suitableTables = tableUsage.TimeSlots.SelectMany(s => s.TableSlots.Keys).Distinct().ToList();

            foreach (var slot in reservationSlots)
            {
                var tableSlot = tableUsage.TimeSlots.FirstOrDefault(a => a.Value == slot);
                var suitableSlotTables = tableSlot.TableSlots.Values.Where(a => a.ReservationId == null && a.Capacity >= reservation.Persons);
                suitableTables = suitableTables.Union(suitableSlotTables.Select(s => s.TableId)).ToList();
            }

            // select right suitableTable

            Dictionary<int, List<TableSlot>> continuoseSlotLeft = new Dictionary<int, List<TableSlot>>();
            var nextTimeSlot = reservationSlots.Last() + slotDuration.ToShortInt();
            suitableTables.ForEach(s => continuoseSlotLeft.Add(s,new List<TableSlot>()));
            foreach (var tableTimeSlot in tableUsage.TimeSlots.SkipWhile(s => s.Value < nextTimeSlot))
            {
                var usableTableCount = 0;
                foreach (var suitableTable in suitableTables)
                {
                    if((tableTimeSlot.TableSlots[suitableTable].ReservationId == null))
                    {
                        usableTableCount++;
                        continuoseSlotLeft[suitableTable].Add(tableTimeSlot.TableSlots[suitableTable]);
                    }
                }

                if (usableTableCount == 0)
                {
                    break;
                }
            }

            return continuoseSlotLeft.Max(a => a.Value.Count);

        }

        private IEnumerable<Reservation> TodayReservation(DateTime date, Models.Db.Place place, out List<Reservation> todayActiveReservation)
        {
            var todayReservation = db.Reservations
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

        public IEnumerable<Reservation> GetReservations(DateTime from, DateTime to)
        {
            var person = personService.GetCurentPerson();

            return db.Reservations.Where(a => a.PersonId == person.Id &&
                                                    DbFunctions.TruncateTime(a.Time) >= from && DbFunctions.TruncateTime(a.Time) <= to);
        }

        public async Task<Reservation> GetReservation(int id)
        {
            return await db.Reservations.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> UpdateReservationState(int id, ReservationState state)
        {
            var reservation = await db.Reservations.FirstOrDefaultAsync(a => a.Id == id);
            if (reservation == null) return false;
            reservation.Status = state;
            db.Reservations.AddOrUpdate(reservation);
            await db.SaveChangesAsync();
            return true;
        }

        private IEnumerable<TimeSlot> GetTimeSlots(DateTime date, BusinessHours placeTime)
        {
            var result = new List<TimeSlot>();
            var startTime = placeTime.OpenTine;
            var nowTime = DateTime.Now.RoundUp(new TimeSpan(0, 30, 0)).TimeOfDay;
          

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
                startTime = startTime + slotDuration;
            }

            return result;
        }

        private List<TableTimeSlot> CreateTableTable(
            ICollection<Seat> seats,
            ICollection<Reservation> reservations,
            ICollection<TimeSlot> timeSlot,
            bool include)
        {
            var result = new List<TableTimeSlot>();
            var placeCapacity = seats.Sum(s => s.Capacity);

            foreach (var slot in timeSlot)
            {
                var reservation = reservations.Where(
                        r => r.Time.ToShortInt() < slot.Value + slotDuration.ToShortInt() && slot.Value < r.Time.ToShortInt() + r.Duration.ToShortInt())
                    .ToList();
                var tableSlot = new TableTimeSlot(slot);
                foreach (var seat in seats)
                {
                    var seatReservation = reservation.FirstOrDefault(r => r.Seats.Select(s => s.Id).Contains(seat.Id));
                    tableSlot.TableSlots.Add(seat.Id, new TableSlot
                    {
                        Capacity = seat.Capacity,
                        ReservationId = seatReservation?.Id,
                        Used = seatReservation?.Persons ?? 0,
                        TableId = seat.Id
                    });
                }

                tableSlot.MaxTable = tableSlot.TableSlots.Values.Max(s => s.Capacity - s.Used);
                tableSlot.CapacityLeft = tableSlot.TableSlots.Values.Sum(s => s.Capacity - s.Used);
                tableSlot.Reserved = slot.CapacityLeft <= 0;
               
                result.Add(tableSlot);
            }

            return result;
        }
    }

    public class ReservationUsageDto
    {
        public  List<TimeSlot> TimeSlots { get; set; }
    }

    public class ReservationUsage
    {
        public List<TableTimeSlot> TimeSlots { get; set; }
    }

    public class ReservationDataDto
    {
        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int SlotCount { get; set; }

        public int MinimalReservation { get; set; }
    }
}