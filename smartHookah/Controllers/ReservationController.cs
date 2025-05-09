﻿using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Redis;
using smartHookah.Services.Messages;
using smartHookah.Services.Redis;
using smartHookah.Support;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    using smartHookah.Models.Dto.Reservations;
    using smartHookah.Services.Place;

    [System.Web.Mvc.Authorize]
    public class ReservationController : Controller
    {
        private readonly IReservationService reservationService;
        private TimeSpan _slotDuration = new TimeSpan(0, 30, 0);
        private readonly SmartHookahContext db;
        private EmailService emailService;
        private readonly ISignalNotificationService _signalNotificationService;
        private readonly IRedisService redisService;
        private readonly IPlaceService placeService;
        public ReservationController(SmartHookahContext db, IReservationService reservationService, ISignalNotificationService signalNotificationService, IPlaceService placeService)
        {
            this.db = db;
            this.reservationService = reservationService;
            this.emailService = new EmailService();
            this._signalNotificationService = signalNotificationService;
            this.placeService = placeService;
        }


        // GET: Reservation
        public ActionResult Index()
        {
            var places = db.Places.Where(a => a.HaveReservation);
            return View(places);
        }

        public ActionResult Create(string id)
        {
            var place = db.Places.Where(a => a.FriendlyUrl == id).Include(a => a.Seats).FirstOrDefault();

            var model = new CreateReservationModel();
            model.place = place;

            return View(model);
        }

        public void Ping(int id)
        {
            this._signalNotificationService.ReservationChanged(id);
        }

        public ActionResult Details(int id)
        {
            var person = UserHelper.GetCurentPerson(db);

            if (person == null)
                return RedirectToAction("Index");

            var reservation = db.Reservations.Where(a => a.Id == id && a.PersonId == person.Id).Include(a => a.Orders)
                .FirstOrDefault();

            var model = new ReservationDetailsViewModel();
            model.Reservation = reservation;
            if (model.Reservation != null)
            {
                var smokeSessions = model.Reservation.Orders.Where(a => a.SmokeSession != null)
                    .Select(a => a.SmokeSession);

                var activeSessions = smokeSessions.Where(a => a.Hookah.Code == a.SessionId);

                var doneSessions = smokeSessions.Except(activeSessions);

                model.ActiveSession = activeSessions;
                model.DoneSession = doneSessions;

                model.ActiveOrders =
                    reservation.Orders.Where(a => a.State == OrderState.Open || a.State == OrderState.Processing);

                model.DynamicSmokeSession = activeSessions.ToDictionary(a => a.SessionId,
                    a => this.redisService.GetDynamicSmokeStatistic(sessionId: a.SessionId));
            }
            return View(model);
        }

        public async Task<ActionResult> Place(string id)
        {

            var place = db.Places.Where(a => a.FriendlyUrl == id).Include(a => a.Seats).FirstOrDefault();
            var canManage = await this.placeService.CanManagePlace(place.Id);

            if (!canManage)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound, "Place cannot be managed");
            }

            var model = new CreateReservationModel();
            model.place = place;

            return View(model);
        }


        [HttpPost]
        public async Task<JsonResult> PostReservation(int id, PostReservationModel model)
        {
            //TODO
            //using (var scope =
            using (var scope = db.Database.BeginTransaction())
            {
                try
                {
                    var parseDate = DateTime.ParseExact(model.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    var duration = _slotDuration.Multiply(model.Duration);
                    var table = db.Seats.FirstOrDefault(a => a.Id == model.Table);
                    var reservation = table?.Reservations.Where(a => a.Time.Date == parseDate &&
                                                                     a.Status != ReservationState.Canceled
                                                                     && a.Status != ReservationState.Denied
                                                                     && a.Status != ReservationState.NonVisited).ToList();

                    var place = await db.Places.FindAsync(id);
                    if (place == null || reservation == null)
                        return Json(new { success = false });

                    var time = DateTime.ParseExact(model.Time.ToString().PadLeft(4, '0'), "HHmm", CultureInfo.InvariantCulture);
                    parseDate = parseDate.AddHours(time.Hour);
                    parseDate = parseDate.AddMinutes(time.Minute);
                    var status = ReservationState.Created;

                    if (model.Persons > table.Capacity)
                        status = ReservationState.ConfirmationRequired;

                    var person = UserHelper.GetCurentPerson(db);

                    if (place.Managers.Any(a => a.Id == person.Id))
                    {
                        status = ReservationState.Confirmed;
                    }

                    var newReservation = new Reservation
                    {
                        Created = DateTime.Now,
                        Person = person,
                        Persons = model.Persons,
                        PlaceId = id,
                        Status = status,
                        Time = parseDate,
                        Seats = new List<Seat>
                        {
                            table
                        },
                        Duration = duration,
                        Text = model.Text,
                        Customers = new List<Person>() { person },
                        Name = person.DisplayName
                    };

                    if (!string.IsNullOrEmpty(model.Name))
                        newReservation.Name = model.Name;


                    //scope.Complete();
                    var conflict = false;
                    var notConfirm = false;
                    foreach (var reservation1 in reservation)
                    {
                        // Check if new reservation is not in conflick
                        conflict = Colide(reservation1, newReservation);
                        if (conflict)
                        {
                            // Check if is in conflick with uncofirm reservation
                            if (reservation1.Status == ReservationState.ConfirmationRequired)
                                notConfirm = true;
                            else
                            {
                                notConfirm = false;
                                break;
                            }
                        }
                    }

                    // If is in conflict in non confirm reservation, change status to confirmation requered as well
                    if (notConfirm)
                    {
                        conflict = false;
                        newReservation.Status = ReservationState.ConfirmationRequired;
                    }

                    if (!conflict)
                    {
                        db.Reservations.Add(newReservation);

                        await db.SaveChangesAsync();

                        SendReservationMail(newReservation);
                    }

                    scope.Commit();
                    this._signalNotificationService.ReservationChanged(newReservation);
                    await reservationService.UpdateReservationUsage(place.Id, parseDate.Date);
                    return Json(new { success = !conflict, id = newReservation.Id });
                }
                catch (Exception e)
                {
                    scope.Rollback();
                    Console.WriteLine(e);
                }
                finally
                {
                    //scope.Dispose();
                }
            }
            return null;
        }

        private void SendReservationMail(Reservation newReservation)
        {

            if (newReservation.Status == ReservationState.Created)
            {
                SendReservationCreatemMail(newReservation);
            }

            // DEBUG
            if (newReservation.Status == ReservationState.Confirmed)
            {
                SendReservationCreatemMail(newReservation);
            }

            if (newReservation.Status == ReservationState.ConfirmationRequired)
            {
                SendReservationConfirmRequiredMail(newReservation);
            }
        }

        private void SendReservationCreatemMail(Reservation newReservation)
        {
            var email = newReservation.getEmail();
            emailService.SendTemplateAsync(email, "Potvrzení rezervace", "reservationConfirmManual.cshtml", newReservation);

        }

        private void SendReservationConfirmMail(Reservation newReservation)
        {
            var email = newReservation.getEmail();
            emailService.SendTemplateAsync(email, "Potvrzení rezervace", "reservationConfirmManual.cshtml", newReservation);
        }

        private void SendReservationDeniedMail(Reservation newReservation)
        {

            var email = newReservation.getEmail();
            emailService.SendTemplateAsync(email, "Zamítnutí rezervace", "reservationDenied.cshtml", newReservation);

        }

        private void SendReservationConfirmRequiredMail(Reservation newReservation)
        {
            var email = newReservation.getEmail();
            emailService.SendTemplateAsync(email, "Rezervace čeká na potvrzení", "reservationWaitForConfirm.cshtml", newReservation);
        }

        private bool CheckReservation(List<Reservation> reservation, Reservation newReservation)
        {
            return false;
        }


        public async Task<JsonResult> Reservations(int id, string date)
        {
            var model = GetReservationModel(id, date);


            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> PlaceReservation(int id, string date)
        {
            var model = GetReservationModel(id, date, true);


            return Json(model, JsonRequestBehavior.AllowGet);
        }


        private ReservationInfo GetReservationModel(int id, string date, bool includeReservation = false)
        {
            var place = db.Places.Where(a => a.Id == id).Include(a => a.Seats).FirstOrDefault();

            var parseDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture);

            var todayDay = (int)parseDate.DayOfWeek;
            var timeSlot = new List<TimeSlot>();

            var placeTime = place.BusinessHours.FirstOrDefault(a => a.Day == todayDay);
            var model = new ReservationInfo();
            if (placeTime != null)
            {
                var startTime = placeTime.OpenTine;
                var nowTime = DateTime.Now.RoundUp(new TimeSpan(0, 30, 0)).TimeOfDay;
                if (parseDate.Date == DateTime.Now.Date && !includeReservation)
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
                    timeSlot.Add(new TimeSlot { Value = startTime.ToShortInt(), Text = startTime.ToString(@"hh\:mm"), OrderIndex = index });
                    index++;
                    startTime = startTime + this._slotDuration;
                }



                var todayReservation =
                    db.Reservations.Where(a => a.PlaceId == place.Id && DbFunctions.TruncateTime(a.Time) == parseDate)
                        .ToList();

                var todayActiveReservation = todayReservation
                    .Where(a => a.Status != ReservationState.Canceled && a.Status != ReservationState.Denied && a.Status != ReservationState.NonVisited).ToList();



                model.Times = new List<KeyValuePair<int, string>>();

                var minReservationTime = place.MinimumReservationTime;
                model.MinimumReservationTime = place.MinimumReservationTime;
                if (includeReservation)
                {
                    minReservationTime = 1;
                }

                for (int i = minReservationTime; i < 9; i++)
                {
                    var text = $"{ i * 0.5} hodiny";
                    model.Times.Add(new KeyValuePair<int, string>(i, text));
                }

                if (includeReservation)
                {
                    model.Times.Add(new KeyValuePair<int, string>(index, "Do zavíračky"));
                }


                model.Tables = new List<TableDto>(place.Seats
                    .Select(a => new TableDto { Id = a.Id, Capacity = a.Capacity, Name = a.Name }).ToList());

                if (includeReservation)
                {
                    model.TodayReservation =
                        todayActiveReservation.ToDictionary(a => a.Id.ToString(),
                            a => new ReservationDto(a, _slotDuration));
                    model.Canceled = todayReservation
                        .Where(a => a.Status == ReservationState.Canceled || a.Status == ReservationState.Denied || a.Status == ReservationState.NonVisited)
                        .Select(a => new ReservationDto(a, _slotDuration));

                    model.ConfirmationRequired = todayActiveReservation.Where(a => a.Status == ReservationState.ConfirmationRequired).Select(a => new ReservationDto(a, _slotDuration));
                }


                model.Reservations = new Dictionary<string, List<TimeSlot>>();

                foreach (var table in model.Tables)
                {
                    var tableReservation = todayActiveReservation.Where(a => a.Seats.Any(b => b.Id == table.Id));

                    var timeTable = CreateTableTable(tableReservation, timeSlot, includeReservation);
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

        public async Task<JsonResult> ChangeReservation(int id, PostReservationModel model, int tableId)
        {
            var reservation = await db.Reservations.FindAsync(id);

            if (reservation == null)
                return Json(new { success = false, msg = "Reservation not found" });

            var oldReservation = new ReservationDto(reservation, this._slotDuration);

            if (reservation.Persons != model.Persons)
                reservation.Persons = model.Persons;

            var parseDate = DateTime.ParseExact(model.Date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var duration = _slotDuration.Multiply(model.Duration);

            var table = db.Seats.FirstOrDefault(a => a.Id == model.Table);

            var time = DateTime.ParseExact(model.Time.ToString().PadLeft(4, '0'), "HHmm", CultureInfo.InvariantCulture);
            parseDate = parseDate.AddHours(time.Hour);
            parseDate = parseDate.AddMinutes(time.Minute);

            if (reservation.Time != parseDate)
                reservation.Time = parseDate;

            if (reservation.Duration != duration)
                reservation.Duration = duration;

            if (reservation.Text != model.Text && !string.IsNullOrEmpty(model.Text))
                reservation.Text = model.Text;

            if (reservation.Name != model.Name && !string.IsNullOrEmpty(model.Name))
                reservation.Name = model.Name;


            if (reservation.Seats.All(a => a.Id != model.Table))
            {
                var changedSeat = reservation.Seats.FirstOrDefault(a => a.Id == tableId);
                reservation.Seats.Remove(changedSeat);

                var tableReservation = table.Reservations.Where(a =>
                    a.Time.Date == reservation.Time.Date && a.Status != ReservationState.Canceled &&
                    a.Status != ReservationState.Denied && a.Status != ReservationState.NonVisited);

                foreach (var res in tableReservation)
                    if (Colide(reservation, res))
                        return Json(new { success = false, msg = $"Reservation colided with {res.Name}" });

                reservation.Seats.Add(table);
            }

            db.Reservations.AddOrUpdate(reservation);
            await db.SaveChangesAsync();
            this._signalNotificationService.ReservationChanged(reservation);
            return Json(new
            {
                oldRes = oldReservation,
                newRes = new ReservationDto(reservation, this._slotDuration),
                success = true
            });
        }


        private bool Colide(Reservation reservation, Reservation tableReservation)
        {
            //Cant colide with self
            if (reservation.Id == tableReservation.Id)
                return false;
            var firstTimes = TakenTime(reservation, _slotDuration, true).Select(a => a.Value);
            var secondTimes = TakenTime(tableReservation, _slotDuration, true).Select(a => a.Value);

            var intersect = firstTimes.Intersect(secondTimes);

            return intersect.Any();
        }

        private List<TimeSlot> CreateTableTable(IEnumerable<Reservation> tableReservation, List<TimeSlot> timeSlot,
            bool include)
        {
            var takenTime = tableReservation.SelectMany(a => TakenTime(a, _slotDuration, include));

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

    public class ReservationDetailsViewModel
    {
        public ReservationDetailsViewModel()
        {
            ActiveSession = new List<SmokeSession>();
            DoneSession = new List<SmokeSession>();
            ActiveOrders = new List<HookahOrder>();
        }

        public Reservation Reservation { get; set; }

        public IEnumerable<SmokeSession> ActiveSession { get; set; }
        public IEnumerable<SmokeSession> DoneSession { get; set; }
        public IEnumerable<HookahOrder> ActiveOrders { get; set; }
        public Dictionary<string, DynamicSmokeStatistic> DynamicSmokeSession { get; set; }
    }

    internal class Compare : IEqualityComparer<TimeSlot>
    {
        public bool Equals(TimeSlot x, TimeSlot y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode(TimeSlot codeh)
        {
            return codeh.Value.GetHashCode();
        }
    }

    public class PostReservationModel
    {
        public int Persons { get; set; }
        public int Table { get; set; }
        public string Date { get; set; }
        public int Time { get; set; }
        public int Duration { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }
    }

    public class TableDto
    {
        public int Id { get; set; }
        public int Capacity { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// The reservation dto.
    /// </summary>
    public class ReservationDto
    {
        /// <summary> 
        /// Initializes a new instance of the <see cref="ReservationDto"/> class.
        /// </summary>
        /// <param name="res">
        /// The res.
        /// </param>
        /// <param name="slot">
        /// The slot.
        /// </param>
        public ReservationDto(Reservation res, TimeSpan slot)
        {
            this.Name = res.DisplayName;
            this.TimeSlots = (int)(res.Duration.Ticks / slot.Ticks);
            this.Id = res.Id;
            this.Persons = res.Persons;
            this.State = res.Status;
            this.Message = res.Text;
            this.TimeText = res.Time.TimeOfDay.ToString(@"hh\:mm");
            this.LateText = (res.LateDuration != null ? "+ " + res.LateDuration.ToString() + " min" : "");
            this.StateText = res.Status.ToString().ToLower();
        }

        /// <summary>
        /// Gets or sets the state text.
        /// </summary>
        public string StateText { get; set; }

        /// <summary>
        /// Gets or sets the time text.
        /// </summary>
        public string TimeText { get; set; }

        public string LateText { get; set; }

        /// <summary>
        /// Gets or sets  reservation message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the time slots.
        /// </summary>
        public int TimeSlots { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the persons.
        /// </summary>
        public int Persons { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public ReservationState State { get; set; }
    }

    /// <summary>
    /// The create reservation model.
    /// </summary>
    public class CreateReservationModel
    {
        public Place place { get; set; }
    }

    public class ReservationInfo
    {
        public Dictionary<string, List<TimeSlot>> Reservations { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
        public List<TableDto> Tables { get; set; }
        public Dictionary<string, ReservationDto> TodayReservation { get; set; }
        public IEnumerable<ReservationDto> Canceled { get; set; }

        public List<KeyValuePair<int, string>> Times { get; set; }

        public int MinimumReservationTime { get; set; }
        public IEnumerable<ReservationDto> ConfirmationRequired { get; set; }
    }

}
