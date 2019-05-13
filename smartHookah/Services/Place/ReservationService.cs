using System.Data.Entity.Migrations;
using smartHookah.Models.Db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Places.Reservations;
using smartHookah.Models.Dto.Reservations;
using smartHookah.Services.Messages;
using smartHookah.Services.Person;
using smartHookah.Services.Redis;
using smartHookah.Support;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Services.Place
{
    public class ReservationService : IReservationService
    {
        private readonly SmartHookahContext db;
        private readonly TimeSpan slotDuration = new TimeSpan(0, 30, 0);
        private IReservationEmailService emailService;

        private readonly IRedisService redisService;

        private readonly IPersonService personService;

        private readonly IPlaceService placeService;

        private readonly ISignalNotificationService _signalNotificationService;

        private readonly IFirebaseNotificationService firebaseNotificationService;

        public ReservationService(SmartHookahContext db, IReservationEmailService emailService, IPlaceService placeService,
            IPersonService personService, IRedisService redisService, ISignalNotificationService signalNotificationService, IFirebaseNotificationService firebaseNotificationService)
        {
            this.db = db;
            this.emailService = emailService;
            this.placeService = placeService;
            this.personService = personService;
            this.redisService = redisService;
            this._signalNotificationService = signalNotificationService;
            this.firebaseNotificationService = firebaseNotificationService;
        }

        public async Task<ReservationManageDto> GetReservationManage(int id, DateTime date)
        {
            var place = await placeService.GetManagedPlace(id);
            var tables = SeatDto.FromModelList(place.Seats);
            var reservations = TodayReservation(date, place, out var today);
            var placeTime = place.BusinessHours.First(a => a.Day == (int) date.DayOfWeek);
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
            var person = personService.GetCurentPerson(this.db);
            reservation.PersonId = personService.GetCurentPerson().Id;
            reservation.Created = DateTime.UtcNow;
            reservation.Started = null;
            var modelReservation = ReservationDto.ToModel(reservation);
            modelReservation.Status = ReservationState.ConfirmationRequired;
            modelReservation.Customers = new List<Models.Db.Person>();
            if (person.Place?.Id != person.Id || person.Id == 1)
            {
                modelReservation.Customers = new List<Models.Db.Person>(){ person };
            }
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
            this._signalNotificationService.ReservationChanged(dbReservation);
            this.emailService.CreatedReservation(dbReservation);
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

            var reservations =
                db.Reservations.Where(a => a.PlaceId == place.Id && EntityFunctions.TruncateTime(a.Time) == date);
            reservations = GetActiveReservations(reservations);
            var reservationsList = await reservations.ToListAsync();

            var timesSlots = GetTimeSlots(
                date,
                place.BusinessHours.First(a => a.Day == (int) date.DayOfWeek));

            var tableData = CreateTableTable(place.Seats, reservations.ToList(), timesSlots.ToList(), true);

            var reservationUsage = new ReservationUsage {TimeSlots = tableData};

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
            var takenSlots = Math.Round(reservation.Duration.Ticks / (double) slotDuration.Ticks);
            return slots.Take((int) takenSlots).Select(s => s.Value);
        }

        private async Task<int?> AllocateSeat(Reservation reservation)
        {
            var place = await db.Places.FindAsync(reservation.PlaceId);
            if (place == null)
            {
                return null;
            }

            var tableUsage = await this.GetReservationUsage(place.Id, reservation.Time.Date);

            var reservationSlots = getReservationTimeSlots(reservation,
                place.BusinessHours.FirstOrDefault(s => s.Day == (int) reservation.Time.DayOfWeek)).ToList();

            if (reservationSlots.Count() > (int) Math.Round(reservation.Duration.Ticks / (double) slotDuration.Ticks))
            {
                return null;
            }

            var suitableTables = tableUsage.TimeSlots.SelectMany(s => s.TableSlots.Keys).Distinct().ToList();

            foreach (var slot in reservationSlots)
            {
                var tableSlot = tableUsage.TimeSlots.FirstOrDefault(a => a.Value == slot);
                var suitableSlotTables =
                    tableSlot.TableSlots.Values.Where(a =>
                        a.ReservationId == null && a.Capacity >= reservation.Persons);
                suitableTables = suitableTables.Intersect(suitableSlotTables.Select(s => s.TableId)).ToList();
            }

            if (suitableTables.Count == 1)
            {
                return suitableTables.Single();
            }

            if (suitableTables.Count == 0)
            {
                return null;
            }

            // select right suitableTable

            Dictionary<int, List<TableSlot>> continuoseSlotLeft = new Dictionary<int, List<TableSlot>>();
            var nextTimeSlot = reservationSlots.Last() + slotDuration.ToShortInt();
            suitableTables.ForEach(s => continuoseSlotLeft.Add(s, new List<TableSlot>()));
            foreach (var tableTimeSlot in tableUsage.TimeSlots.SkipWhile(s => s.Value < nextTimeSlot))
            {
                var usableTableCount = 0;
                foreach (var suitableTable in suitableTables)
                {
                    if ((tableTimeSlot.TableSlots[suitableTable].ReservationId == null))
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

            var continuoseSlot = continuoseSlotLeft.Max(a => a.Value.Count);
            return continuoseSlotLeft.FirstOrDefault(a => a.Value.Count == continuoseSlot).Key;
        }

        private IEnumerable<Reservation> TodayReservation(DateTime date, Models.Db.Place place,
            out List<Reservation> todayActiveReservation)
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
                                              DbFunctions.TruncateTime(a.Time) >= from &&
                                              DbFunctions.TruncateTime(a.Time) <= to);
        }

        public async Task<Reservation> AddLateTime(int id, int time)
        {
            var reservation = await db.Reservations.FindAsync(id);

            if (reservation == null)
            {
                // Bad reservation
                throw new ManaException(ErrorCodes.ReservationNotFound, $"Reservation with id {id} was not found");
            }

            reservation.LateDuration = time;

            this.db.Reservations.AddOrUpdate(reservation);
            this._signalNotificationService.ReservationChanged(reservation);

            await this.db.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation> GetReservation(int id)
        {
            return await db.Reservations.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Reservation> AddTable(int id, int tableId)
        {
            var reservation = await db.Reservations.FindAsync(id);

            if (reservation == null)
            {
                // Bad reservation
                throw new ManaException(ErrorCodes.ReservationNotFound,$"Reservation with id {id} was not found");
            }

            var table = await db.Seats.FindAsync(tableId);

            if (table == null)
            {
                // Bad reservation
                throw new ManaException(ErrorCodes.TableNotFound, $"Table with id {tableId} was not found");
            }


            var tableReservations = table.Reservations.Where(a =>
                a.Time.Date == reservation.Time.Date && a.Status != ReservationState.Canceled &&
                a.Status != ReservationState.Denied && a.Status != ReservationState.NonVisited);

            foreach (var tableReservation in tableReservations)
                if (Colide(reservation, tableReservation))
                {
                    // Bad reservation
                    throw new ManaException(ErrorCodes.ReservationConflict, $"Reservation with id {id} conflict with another reservation");
                }

            if (reservation.Status == ReservationState.ConfirmationRequired || reservation.Status == ReservationState.Canceled || reservation.Status == ReservationState.NonVisited) 
            {
                reservation.Status = ReservationState.Confirmed;
                this.emailService.CreatedReservation(reservation);
            }

            reservation.Seats.Add(table);

            await db.SaveChangesAsync();
            _signalNotificationService.ReservationChanged(reservation);
            return reservation;
        }

        private bool Colide(Reservation reservation, Reservation tableReservation)
        {
            //Cant colide with self
            if (reservation.Id == tableReservation.Id)
                return false;
            var firstTimes = TakenTime(reservation, slotDuration, true).Select(a => a.Value);
            var secondTimes = TakenTime(tableReservation, slotDuration, true).Select(a => a.Value);

            var intersect = firstTimes.Intersect(secondTimes);

            return intersect.Any();
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

        public async Task<Reservation> RemoveTable(int id, int tableId)
        {
            var reservation = await db.Reservations.FindAsync(id);

            if (reservation == null)
            {
                // Bad reservation
                throw new ManaException(ErrorCodes.ReservationNotFound, $"Reservation with id {id} was not found");
            }

            var table = await db.Seats.FindAsync(tableId);

            if (table == null)
            {
                // Bad table
                throw new ManaException(ErrorCodes.TableNotFound, $"Table with id {tableId} was not found");
            }

            if (!reservation.Seats.Contains(table))
            {
                // Bad remove table
                throw new ManaException(ErrorCodes.TableNotFound, $"Table with id {tableId} was not found on reservation with id {id}");
            }


            reservation.Seats.Remove(table);

            if (reservation.Seats.Count == 0)
            {
                reservation.Status = ReservationState.UnConfirmed;
                this.emailService.CreatedReservation(reservation);
            }
            
            await db.SaveChangesAsync();
            _signalNotificationService.ReservationChanged(reservation);
            return reservation;
        }

        public async Task<Reservation> UpdateLateDuration(int id, int lateDuration)
        {
            var reservation = await this.db.Reservations.FindAsync(id);

            if (reservation == null)
            {
                // Bad reservation
                throw new ManaException(ErrorCodes.ReservationNotFound, $"Reservation with id {id} was not found");
            }

            reservation.LateDuration = lateDuration;
            this.db.Reservations.AddOrUpdate(reservation);
            await this.db.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> UpdateReservationState(int id, ReservationState state)
        {
            var reservation = await db.Reservations.FirstOrDefaultAsync(a => a.Id == id);
            if (reservation == null)
            {
                throw new ManaException(ErrorCodes.ReservationNotFound,$"Reservation with id {id} was not found");
            }
            reservation.Status = state;
            var person = this.personService.GetCurentPerson();
            var placeManagersId = reservation.Place.Managers.Select(s => s.Id);
            if (!placeManagersId.Contains(person.Id))
            {
                if (state != ReservationState.Canceled)
                {
                    throw new ManaException(ErrorCodes.ReservationStateRole, $"Reservation state for user can be only canceled");
                }
            }

            if (state == ReservationState.Canceled || state == ReservationState.NonVisited)
            {
                reservation.Seats.Clear();
               
            }

            db.Reservations.AddOrUpdate(reservation);
            await db.SaveChangesAsync();
            this._signalNotificationService.ReservationChanged(reservation);
            this.emailService.StateChanged(reservation);
            if (state == ReservationState.Canceled)
            {
                // todo textace
                await firebaseNotificationService.NotifyAsync(reservation.PersonId.Value, "Reservation canceled",
                    $"Your reservation in ${reservation.Place.Name} on ${reservation.Time.ToString("dd.MM.yyyy hh:mm",CultureInfo.CurrentCulture)} was canceled", new Dictionary<string, string>{{"RESERVATION_ID",reservation.Id.ToString()}});
            }

            if (state == ReservationState.Confirmed)
            {
                // todo textace
                await firebaseNotificationService.NotifyAsync(reservation.PersonId.Value, "Reservation confirmed",
                    $"Your reservation in ${reservation.Place.Name} on ${reservation.Time.ToString("dd.MM.yyyy hh:mm", CultureInfo.CurrentCulture)} was confirmed", new Dictionary<string, string> { { "RESERVATION_ID", reservation.Id.ToString() } });
            }
            
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
                    new TimeSlot
                        {Value = startTime.ToShortInt(), Text = startTime.ToString(@"hh\:mm"), OrderIndex = index});
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
                        r => r.Time.ToShortInt() < slot.Value + slotDuration.ToShortInt() &&
                             slot.Value < r.Time.ToShortInt() + r.Duration.ToShortInt())
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
                tableSlot.TablesLeft = tableSlot.TableSlots.Values.Count(a => a.ReservationId == null);
                tableSlot.CapacityLeft = tableSlot.TableSlots.Values.Sum(s => s.Capacity - s.Used);
                tableSlot.Reserved = slot.CapacityLeft <= 0;

                result.Add(tableSlot);
            }

            return result;
        }
    }

    public class ReservationUsageDto
    {
        public List<TimeSlot> TimeSlots { get; set; }
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