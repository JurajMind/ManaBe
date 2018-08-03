namespace smartHookah.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Spatial;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;

    using GuigleAPI;

    using smartHookah.Helpers;
    using smartHookah.Models;
    using smartHookah.Models.Redis;
    using smartHookah.Support;

    using smartHookahCommon;

    /// <summary>
    ///     The places controller.
    /// </summary>
    public partial class PlacesController : Controller
    {
        /// <summary>
        ///     The db.
        /// </summary>
        private readonly SmartHookahContext db;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlacesController" /> class.
        /// </summary>
        /// <param name="db">
        ///     The db.
        /// </param>
        public PlacesController(SmartHookahContext db)
        {
            this.db = db;
        }

        public ActionResult AddMedia(int id)
        {
            Place place;
            Person person;
            var canManage = this.canManagePlace(out place, id, out person);

            if (canManage) return this.View(place);
            return this.RedirectToAction("Index");
        }

        [System.Web.Http.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Http.ActionName("AddMedia")]
        public async Task<ActionResult> AddMediaPost(int id, HttpPostedFileBase file)
        {
            var place = await this.db.Places.FindAsync(id);
            var path = $"/Content/Place/";
            if (file != null)
            {
                var media = new Media();
                var lastId = place.Medias.OrderBy(a => a.Id).Select(a => a.Id).DefaultIfEmpty(0).First() + 1;
                var extension = Path.GetExtension(file.FileName);
                var scalePath = path + place.FriendlyUrl + "/" + lastId;
                media.Path = scalePath + extension;
                media.Created = DateTime.Now;
                Directory.CreateDirectory(this.Server.MapPath(path + place.FriendlyUrl));

                var sourceimage = Image.FromStream(file.InputStream);
                MediaController.ScaleAndSave(sourceimage, 180, 200, scalePath, this.Server);

                file.SaveAs(this.Server.MapPath(media.Path));
                place.Medias.Add(media);
                this.db.Places.AddOrUpdate(place);
                await this.db.SaveChangesAsync();
            }

            return this.RedirectToAction("Media", "Places", new { id = place.Id });
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> CancelOrder(int id)
        {
            return await this.ChangeOrderState(id, OrderState.Canceled);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> ChangeOrderState(int id, int state)
        {
            return await this.ChangeOrderState(id, (OrderState)state);
        }

        [System.Web.Mvc.Authorize]

        // GET: Lounge/Create
        public ActionResult Create()
        {
            return this.View();
        }

        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> Create(
            [Bind(Include = "Id,Name,FriendlyUrl,LogoPath,Descriptions,ShortDescriptions,Address,PhoneNumber,Facebook")]
            Place lounge,
            HttpPostedFileBase file)
        {
            if (this.ModelState.IsValid)
            {
                if (file != null)
                {
                    var path = $"/Content/PlacePictures/";
                    var extension = Path.GetExtension(file.FileName);
                    lounge.LogoPath = path + lounge.FriendlyUrl + extension;
                    lounge.Address = await this.GetLocation(lounge.Address);
                    file.SaveAs(this.Server.MapPath(lounge.LogoPath));
                }

                this.db.Places.Add(lounge);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return this.View(lounge);
        }

        public ActionResult DashBoard(int? id)
        {
            return this.View(id);
        }

        [System.Web.Mvc.Authorize]
        public async Task<JsonResult> DashBoardData(int? id)
        {
            var place = this.db.Places.Where(a => a.Id == id).Include(a => a.Person).Include(a => a.Person.Hookahs)
                .FirstOrDefault();

            var model = new LoungeDashBoardViewModel();

            if (place == null) return this.Json(model, JsonRequestBehavior.AllowGet);

            var hookahs = place.Person.Hookahs.ToList();

            var onlineHookah = await IotDeviceHelper.GetState(hookahs.Select(a => a.Code).ToList());
            model.Place = new Place();
            model.Place.Id = place.Id;
            var DynamicStatistic = SmokeSessionController.GetDynamicSmokeStatistic(hookahs, a => a.Code);

            model.Hookah = new List<HookahDashboardViewModel>();

            foreach (var hookah in hookahs)
            {
                var redisSessionId = RedisHelper.GetSmokeSessionId(hookah.Code);
                var sessionMetadata = hookah.SmokeSessions.FirstOrDefault(a => a.SessionId == redisSessionId);
                var estTobaco = 300d;
                var hookahPicture = "/Content/icons/hookah.svg";
                if (sessionMetadata != null && sessionMetadata.MetaData != null)
                {
                    if (sessionMetadata.MetaData.Tobacco != null)
                        estTobaco = sessionMetadata.MetaData.Tobacco.GetTobacoEstimated(this.db);
                    if (sessionMetadata.MetaData.Pipe != null && sessionMetadata.MetaData.Pipe.Picture != null)
                        hookahPicture = sessionMetadata.MetaData.Pipe.Picture;
                }

                DynamicSmokeStatistic dynamicOut;
                if (!DynamicStatistic.TryGetValue(hookah.Code, out dynamicOut))
                    dynamicOut = new DynamicSmokeStatistic();
                model.Hookah.Add(
                    new HookahDashboardViewModel
                        {
                            Key = hookah.Code,
                            Name = hookah.Name,
                            DynamicSmokeStatisticDto =
                                new DynamicSmokeStatisticDto(dynamicOut),
                            Online = onlineHookah.Contains(hookah.Code),
                            EstPufCount = (int)estTobaco,
                            HookahPicture = hookahPicture,
                            Table = RedisHelper.GetHookahSeat(hookah.Code)
                        });
            }

            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        // GET: Lounge/Delete/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var lounge = this.db.Places.Find(id);
            if (lounge == null) return this.HttpNotFound();
            return this.View(lounge);
        }

        // POST: Lounge/Delete/5
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var lounge = this.db.Places.Find(id);
            this.db.Places.Remove(lounge);
            this.db.SaveChanges();
            return this.RedirectToAction("Index");
        }

        public ActionResult DeleteMedia(int id, int mediaId)
        {
            Place place;
            Person person;
            var canManage = this.canManagePlace(out place, id, out person);

            if (canManage)
            {
                var media = this.db.Media.Find(mediaId);
                var serverPath = this.Server.MapPath(media.GetDirectory);
                var dir = new DirectoryInfo(serverPath);

                foreach (var file in dir.EnumerateFiles($"{media.FileName}*")) file.Delete();

                this.db.Media.Remove(media);
                this.db.SaveChanges();

                return this.RedirectToAction("Media", new { id = place.Id });
            }

            return this.RedirectToAction("Index");
        }

        // GET: Places/test
        public ActionResult Details(string id)
        {
            var lounge = this.db.Places.Where(a => a.FriendlyUrl == id).Include(a => a.Person)
                .Include(a => a.Person.OwnedPipeAccesories).FirstOrDefault();
            var model = new PlaceDetailsViewModel();
            if (lounge == null) return this.HttpNotFound();

            var person = UserHelper.GetCurentPerson(this.db, lounge.PersonId);
            model.Place = lounge;
            model.CanEdit = person != null;
            return this.View(model);
        }

        // GET: Places/Details/5
        public ActionResult DetailsById(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var place = this.db.Places.Find(id);
            if (place == null) return this.HttpNotFound();

            var model = new PlaceDetailsViewModel { Place = place };

            return this.View("Details", model);
        }

        // GET: Lounge/Edit/5
        [System.Web.Mvc.Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var lounge = this.db.Places.Find(id);
            if (lounge == null) return this.HttpNotFound();
            return this.View(lounge);
        }

        // POST: Lounge/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize]
        public ActionResult Edit(
            [Bind(Include = "Id,Name,LogoPath,Descriptions,ShortDescriptions,Address")] Place lounge)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(lounge).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            return this.View(lounge);
        }

        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> EditOpenHours(int? id)
        {
            var model = new EditOpenHoursViewModel();
            var place = this.db.Places.Include(b => b.BusinessHours).FirstOrDefault(a => a.Id == id);

            if (place == null) return this.RedirectToAction("Index", "Places");

            var person = UserHelper.GetCurentPerson(this.db, place.Person.Id);

            if (person == null) return this.RedirectToAction("Index", "Places");

            model.BusinessHours = place.BusinessHours.ToList();

            for (var i = 0; i < 7; i++)
                if (model.BusinessHours.All(a => a.Day != i))
                    model.BusinessHours.Add(
                        new BusinessHours { Day = i, OpenTine = new TimeSpan(), CloseTime = new TimeSpan() });
            model.Place = place;
            return this.View(model);
        }

        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOpenHours(EditOpenHoursPostModel model)
        {
            var place = this.db.Places.Include(d => d.BusinessHours).FirstOrDefault(a => a.Id == model.Id);

            if (place == null) return null;

            var person = UserHelper.GetCurentPerson(this.db, place.Person.Id);
            if (person == null) return this.RedirectToAction("Index", "Places");
            foreach (var hour in model.BusinessHours)
            {
                if (string.IsNullOrEmpty(hour.Open) || string.IsNullOrEmpty(hour.Close)) continue;
                var bh = new BusinessHours();
                if (hour.Id == 0)
                {
                    bh = new BusinessHours();
                    bh.Place = place;
                    bh.Day = hour.Day;
                }
                else
                {
                    bh = place.BusinessHours.FirstOrDefault(a => a.Id == hour.Id);
                }

                bh.OpenTine = GetTimeSpan(hour.Open);
                bh.CloseTime = GetTimeSpan(hour.Close);

                this.db.BusinessHours.AddOrUpdate(bh);
            }

            this.db.SaveChanges();

            return this.RedirectToAction("Details", "Places", new { id = place.FriendlyUrl });
        }

        public ActionResult EditPrice(int id)
        {
            var place = this.db.Places.Include(a => a.Person.OwnedPipeAccesories).FirstOrDefault(a => a.Id == id);

            if (place == null) return this.RedirectToAction("Index", "Places");

            var model = new EditPriceViewModel();
            model.OwnedGear = place.Person.OwnedPipeAccesories.OrderBy(a => a.PipeAccesory.BrandName)
                .ThenBy(a => a.PipeAccesory.AccName).ToList();
            model.Extras = place.OrderExtras.ToList();
            model.PlaceId = place.Id;
            model.BasePrice = place.BaseHookahPrice;
            model.Currency = place.Currency;
            model.PriceGroups = place.PriceGroups.ToList();
            return this.View(model);
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> EditPrice(EditPricePostModel model)
        {
            var place = this.db.Places.Include(a => a.Person.OwnedPipeAccesories)
                .FirstOrDefault(a => a.Id == model.PlaceId);

            if (place == null) return this.RedirectToAction("Index", "Places");

            if (place.Currency != model.Currency)
            {
                place.Currency = model.Currency;
                this.db.Places.AddOrUpdate(place);
            }

            foreach (var placePriceGroup in place.PriceGroups)
            {
                var newPriceGroup = model.priceGroup.FirstOrDefault(
                    a => a.Id == placePriceGroup.Id && a.PriceValue != placePriceGroup.Price);
                if (newPriceGroup == null) continue;

                placePriceGroup.Price = newPriceGroup.PriceValue;
                this.db.PriceGroup.AddOrUpdate(placePriceGroup);
            }

            var gear = place.Person.OwnedPipeAccesories.ToList();
            foreach (var item in model.Items)
            {
                var gearItem = gear.FirstOrDefault(a => a.Id == item.Id);
                if (gearItem == null) continue;

                foreach (var price in item.Prices)
                {
                    if (gearItem.Prices == null) gearItem.Prices = new List<PriceGroupPrice>();
                    var priceGroup = gearItem.Prices.FirstOrDefault(a => a.PriceGroupId == price.Id);
                    if (priceGroup == null)
                    {
                        priceGroup = new PriceGroupPrice();
                        priceGroup.PriceGroupId = price.Id;
                        priceGroup.OwnPipeAccesoriesId = gearItem.Id;
                        gearItem.Prices.Add(priceGroup);
                        priceGroup.Price = price.PriceValue;
                        this.db.PriceGroupPrice.AddOrUpdate(priceGroup);
                    }
                    else
                    {
                        if (priceGroup.Price != price.PriceValue)
                        {
                            priceGroup.Price = price.PriceValue;
                            this.db.PriceGroupPrice.AddOrUpdate(priceGroup);
                        }
                    }
                }

                gearItem.Currency = item.Currency;

                // db.OwnPipeAccesorieses.AddOrUpdate(gearItem);
            }

            await this.db.SaveChangesAsync();

            return this.RedirectToAction("Details", new { id = place.FriendlyUrl });
        }

        public async Task<ActionResult> EditSeat(int? id)
        {
            var place = this.db.Places.Include(a => a.Person).Include(a => a.Seats).FirstOrDefault(a => a.Id == id);

            if (place == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new SeatManagerModel();
            model.Seats = place.Seats.ToList();
            model.PlaceId = id.Value;
            return this.View(model);
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.ActionName("EditSeat")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSeat(int id, [FromBody] List<Seat> seat)
        {
            var place = this.db.Places.Include(a => a.Person).Include(a => a.Seats).FirstOrDefault(a => a.Id == id);

            foreach (var s in seat.Where(a => a.Id != 0))
            {
                var placeSeat = place.Seats.FirstOrDefault(a => a.Id == s.Id);

                if (placeSeat == null) continue;

                if (placeSeat.Name == s.Name && placeSeat.Capacity == s.Capacity) continue;

                if (placeSeat.Name != s.Name) placeSeat.Name = s.Name;

                if (placeSeat.Capacity != s.Capacity) placeSeat.Capacity = s.Capacity;

                this.db.Seats.AddOrUpdate(placeSeat);
            }

            foreach (var s in seat.Where(a => a.Id == 0))
            {
                s.Place = place;
                s.PlaceId = place.Id;
                s.Code = Support.RandomString(5);
                this.db.Seats.Add(s);
            }

            await this.db.SaveChangesAsync();

            return this.RedirectToAction("Seat", new { id });
        }

        [System.Web.Mvc.Authorize]
        public async Task<JsonResult> GetOnlineState(int? id)
        {
            var place = this.db.Places.Where(a => a.Id == id).Include(a => a.Person).Include(a => a.Person.Hookahs)
                .FirstOrDefault();

            if (place == null) return this.Json(JsonRequestBehavior.AllowGet);

            var hookahs = place.Person.Hookahs.Select(a => a.Code).ToList();

            var hookahState = new Dictionary<string, string>();

            var online = await IotDeviceHelper.GetState(hookahs);
            var offline = hookahs.Except(online).ToList();

            return this.Json(new { online, offline }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetPlaceMenu(int? id, bool canOrder = false)
        {
            var place = this.db.Places.Include(a => a.Person).Include(a => a.Person.OwnedPipeAccesories)
                .FirstOrDefault(a => a.Id == id);

            var model = new GetPlaceMenuModel();

            if (place != null)
            {
                model.BasePrice = place.BaseHookahPrice;
                model.Hookah = place.Person.OwnedPipeAccesories
                    .Where(a => a.PipeAccesory is Pipe && a.DeleteDate == null).Select(a => new AccesoryDto(a))
                    .ToList();
                if (canOrder) model.Hookah.Insert(0, AccesoryDto.GetDefault());
                model.Bowl = place.Person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Bowl && a.DeleteDate == null)
                    .Select(a => new AccesoryDto(a)).ToList();
                if (canOrder) model.Bowl.Insert(0, AccesoryDto.GetDefault());
                model.Tobacco = place.Person.OwnedPipeAccesories
                    .Where(
                        a => a.PipeAccesory is Tobacco && a.DeleteDate == null
                             && string.IsNullOrEmpty(a.AlternativeName)).Select(a => new AccesoryDto(a)).ToList();

                model.AlternativeTobacco = place.Person.OwnedPipeAccesories
                    .Where(
                        a => a.PipeAccesory is Tobacco && a.DeleteDate == null
                             && !string.IsNullOrEmpty(a.AlternativeName)).Select(a => new AccesoryDto(a)).ToList();

                if (canOrder) model.Tobacco.Insert(0, AccesoryDto.GetDefault());

                model.Mixes = this.db.TobaccoMixs.Where(a => a.AuthorId == place.PersonId && a.AccName != null).ToList()
                    .Select(a => new AccesoryDto(a)).ToList();

                model.Extra = this.db.OrderExtras.Where(a => a.PlaceId == id).ToList().Select(a => new OrderExtraDto(a))
                    .ToList();

                model.PriceGroup = place.PriceGroups.ToList().Select(a => new PriceGroupDto(a)).ToList();

                model.PriceMatrix = new Dictionary<string, Dictionary<string, decimal>>();

                foreach (var pc in model.PriceGroup)
                {
                    var pcMatrix = new Dictionary<string, decimal>();
                    foreach (var item in place.Person.OwnedPipeAccesories)
                    {
                        var priceGroup = item.Prices.FirstOrDefault(a => a.PriceGroupId == pc.Id);
                        if (priceGroup != null) pcMatrix.Add(item.PipeAccesoryId.ToString(), priceGroup.Price);
                    }

                    model.PriceMatrix.Add(pc.Id.ToString(), pcMatrix);
                }

                this.AddDefautlImgPath(model.Bowl, "/Content/icons/bowl.svg");
                this.AddDefautlImgPath(model.Hookah, "/Content/icons/hookah.svg");
            }

            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.Authorize] 
        public async Task<ActionResult> Statistics(string id)
        {
            var place = db.Places.FirstOrDefault(a => a.FriendlyUrl == id);

            return View(place);
        }

        public async Task<JsonResult> GetStatisticData(string id)
        {
            var place = db.Places.Where(a => a.FriendlyUrl ==id)
                .Include(a => a.Reservations)
                .Include("Reservations.Seats").FirstOrDefault();

            var reservation = place.Reservations.Where(
                a => a.Status == ReservationState.VisitConfirmed || a.Status == ReservationState.Confirmed
                     || a.Status == ReservationState.Created);

            var canceled = reservation.Where(a => a.Status == ReservationState.NonVisit);

            var dayDistribution = reservation.GroupBy(a => a.Time.DayOfWeek)
                .ToPlotData(a => a.ToString(),a => (int)a.Key);

            var timeDistribution = reservation.GroupBy(a => a.Time.Hour.ToString(), a => a)
                .ToPlotData(a => a.ToString());

            var groupSize = reservation.GroupBy(a => a.Persons.ToString(), a => a)
                .ToPlotData(a => a.ToString());

            var duration = reservation.GroupBy(a => a.Duration.ToString(), a => a)
                .ToPlotData(a => a.ToString());

            var weekVisits = reservation.GroupBy(a => a.Time.GetIso8601WeekOfYear(), a => a)
                .ToPlotData(a => a.ToString());

            var monthVisit = reservation.GroupBy(a => a.Time.Month, a => a).ToPlotData(a => a.ToString());

            var tableUssage = reservation.GroupBy(a => a.Seats.FirstOrDefault().Name)
                .ToPlotData(a => a.ToString());

            var model = new GetStatisticModel();
            model.dayDistribution = dayDistribution;
            model.timeDistribution = timeDistribution;
            model.groupSize = groupSize;
            model.visitDuration = duration;
            model.weekVisits = weekVisits;
            model.monthVisit = monthVisit;
            model.tableUssage = tableUssage;
            model.customers = reservation.Sum(a => a.Persons);
            var start = reservation.OrderBy(a => a.Created).Select(a => a.Created).FirstOrDefault();
            var end  = reservation.OrderBy(a => a.Created).Select(a => a.Created).FirstOrDefault();
            model.DataSpan = (end.Date - start.Date).TotalDays.ToString();
            model.Start = start.ToString();
            model.ReservationCount = reservation.Count();
            return Json(model,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     The index.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Index()
        {
            var model = new PlaceIndexViewModel();

            var person = UserHelper.GetCurentPerson(this.db);
            model.Places = this.db.Places.ToList();
            if (person != null)
            {
                var ownPlace = model.Places.FirstOrDefault(a => a.Managers.Contains(person));

                if (ownPlace != null) model.ownPlaceId = ownPlace.Id;
            }

            return this.View(model);
        }

        public async Task<ActionResult> Manage(int? id, int? personId)
        {
            var person = UserHelper.GetCurentPerson(this.db, personId);

            if (id == null)
            {
                if (person == null) return this.RedirectToAction("Index");

                var managedPlaces = this.db.Places.Where(a => a.Managers.Any(p => p.Id == person.Id));

                if (managedPlaces.Count() == 1) id = managedPlaces.FirstOrDefault().Id;
                else return this.View("ManagedPlaces", managedPlaces.ToList());
            }

            var place = this.db.Places.Include(a => a.Orders).FirstOrDefault(a => a.Id == id);

            if (place == null) return this.RedirectToAction("Index");

            if (person == null || !place.Managers.Contains(person)) return this.RedirectToAction("Index");

            return this.View(place);
        }

        public ActionResult Media(int id)
        {
            Place place;
            Person person;
            var canManage = this.canManagePlace(out place, id, out person);

            if (canManage) return this.View(place);
            return this.RedirectToAction("Index");
        }

        public async Task<ActionResult> Menu(string id)
        {
            return await this.Order(id, null, null);
        }

        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Order(string id, int? resId = null, string seatId = null)
        {
            if (id == null) return this.RedirectToAction("Index");

            // if (resId == null)
            // {
            // return RedirectToAction("OrderSpecific", new {id = id});
            // }
            var place = this.db.Places.Include(a => a.Person).Include(a => a.Person.OwnedPipeAccesories)
                .Include(a => a.Seats).FirstOrDefault(a => a.FriendlyUrl == id);

            if (place == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new OrderHookahViewModel
                            {
                                Place = place,
                                Hookah = place.Person.Pipes,
                                Bowls = place.Person.Bowls,
                                Tobacco = place.Person.Tobacco
                            };

            if (!string.IsNullOrEmpty(seatId)) resId = RedisHelper.GetReservatopnFromTable(seatId);

            if (resId.HasValue)
            {
                var reservation = this.db.Reservations.Find(resId);
                if (reservation != null && reservation.Seats != null) model.Seat = reservation.Seats;

                if (reservation != null) model.Reservation = reservation;
            }

            if (resId == null) model.CanOrder = false;

            return this.View("Order", model);
        }

        /// <summary>
        ///     The order details.
        /// </summary>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task<ActionResult> OrderDetails(int id)
        {
            var order = await this.db.HookahOrders.FindAsync(id);

            var model = new OrderDetailsViewModel { order = order };
            return this.View(model);
        }

        public ActionResult OrderSpecific(string id)
        {
            if (string.IsNullOrEmpty(id)) this.RedirectToAction("Index");

            var model = new OrderSpecificModel();

            var place = this.db.Places.FirstOrDefault(a => a.FriendlyUrl == id);

            return this.View(place);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> PostOrder(int id, PostOrderModel model)
        {
            var smokeSessionMetadata = new SmokeSessionMetaData();
            var place = await this.db.Places.FindAsync(id);
            if (model.Tobacco.Any(a => a != 0))
            {
                var metadataModel = new SaveSmokeMetadataModel();
                metadataModel.TobacosMix = new List<TobacoMixPart>();
                foreach (var i in model.Tobacco)
                    metadataModel.TobacosMix.Add(new TobacoMixPart { Id = 0, TobaccoId = i, Fraction = 5 });

                var tobacco = TobaccoController.GetTobacoFromMetadata(metadataModel, this.db);
                smokeSessionMetadata.Tobacco = tobacco;
            }

            if (model.Hookah.Any(a => a != 0)) smokeSessionMetadata.PipeId = model.Hookah.First();

            if (model.Bowl.Any(a => a != 0)) smokeSessionMetadata.BowlId = model.Bowl.First();

            var person = UserHelper.GetCurentPerson(this.db);

            var descriptionSb = new StringBuilder();
            descriptionSb.Append("Extra:\n");

            if (model.Extra != null && model.Extra.Any())
            {
                var orderExtra = this.db.OrderExtras.Where(a => model.Extra.Contains(a.Id));

                foreach (var extra in orderExtra)
                {
                    descriptionSb.Append(extra.Name);
                    descriptionSb.Append(Environment.NewLine);
                }
            }

            descriptionSb.Append("Description:");
            descriptionSb.Append(model.Description);
            descriptionSb.Append("TobaccoDescription:");
            descriptionSb.Append(model.TobaccoDescription);
            var order = new HookahOrder
                            {
                                Created = DateTime.Now.ToUniversalTime(),
                                ExtraInfo = descriptionSb.ToString(),
                                SmokeSessionMetaData = smokeSessionMetadata,
                                Person = person,
                                State = OrderState.Open,
                                Place = place
                            };

            if (model.Seat != 0) order.SeatId = model.Seat;

            try
            {
                this.db.SessionMetaDatas.Add(smokeSessionMetadata);
                this.db.HookahOrders.Add(order);
                await this.db.SaveChangesAsync();

                return this.Json(new { success = true, order = order.Id });
            }
            catch (Exception e)
            {
                return this.Json(new { success = false, Error = e.Message });
            }
        }

        /// <summary>
        ///     The process order.
        /// </summary>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        public async Task<ActionResult> ProcessOrder(int id)
        {
            var order = await this.db.HookahOrders.FindAsync(id);

            var model = new ProcessOrderViewModel();
            var person = order.Place.Person;
            model.SmokeMetadataModalViewModel =
                SmokeMetadataModalViewModel.CreateSmokeMetadataModalViewModel(
                    this.db,
                    order.SmokeSessionMetaData,
                    person);
            model.order = order;

            var hookahs = person.Hookahs.ToList();

            var hookahSessionId = RedisHelper.GetSmokeSessionIds(hookahs.Select(a => a.Code).ToList());
            model.Hookahs = new List<Hookah>();
            foreach (var sessionId in hookahSessionId)
            {
                var session = this.db.SmokeSessions.Include(a => a.Pufs).FirstOrDefault(a => a.SessionId == sessionId);

                var pufs = RedisHelper.GetPufs(sessionId);

                if (session == null) continue;

                if (!pufs.Any()) model.Hookahs.Add(session.Hookah);
            }

            model.Seats = order.Place.Seats.ToList();
            return this.View(model);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ProcessOrder(int id, int hookahId, int?seatId, int? reservationId)
        {
            var order = await this.db.HookahOrders.FindAsync(id);
            var hookah = await this.db.Hookahs.FindAsync(hookahId);

            if (order == null || hookah == null) return null;

            var sessionId = RedisHelper.GetSmokeSessionId(hookah.Code);

            var session = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);

            if (seatId != null || seatId != 0) order.SeatId = seatId;

            if (seatId != null || seatId != 0) order.ReservationId = reservationId;

            session.MetaData = order.SmokeSessionMetaData;
            order.SmokeSession = session;

            session.Persons.Add(order.Person);
            order.State = OrderState.Ready;

            this.db.SmokeSessions.AddOrUpdate(session);
            this.db.HookahOrders.AddOrUpdate(order);

            await this.db.SaveChangesAsync();

            return this.RedirectToAction("SmokeSession", "SmokeSession", new { id = session.SessionId });
        }

        public ActionResult PublicDashBoard(int? id)
        {
            return this.View(id);
        }

        public async Task<ActionResult> Seat(int? id)
        {
            var place = this.db.Places.Include(a => a.Person).Include(a => a.Seats).FirstOrDefault(a => a.Id == id);

            if (place == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new SeatManagerModel();
            model.Seats = place.Seats.ToList();
            model.PlaceId = id.Value;
            model.place = place;
            return this.View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) this.db.Dispose();
            base.Dispose(disposing);
        }

        private static TimeSpan GetTimeSpan(string text)
        {
            var dateTime = DateTime.ParseExact(text, "h:mm tt", CultureInfo.InvariantCulture);
            var span = dateTime.TimeOfDay;
            return span;
        }

        private void AddDefautlImgPath(List<AccesoryDto> accesory, string path)
        {
            foreach (var acc in accesory) if (acc.Picture == null) acc.Picture = path;
        }

        private bool canManagePlace(out Place place, int id, out Person person)
        {
            place = this.db.Places.Find(id);

            person = UserHelper.GetCurentPerson(this.db);
            var personId = person.Id;
            var canManage = place.Managers.Any(a => a.Id == personId);
            return canManage;
        }

        private async Task<JsonResult> ChangeOrderState(int id, OrderState state)
        {
            try
            {
                var order = await this.db.HookahOrders.FindAsync(id);

                if (order == null) return this.Json(new { success = false, msg = "Order not found" });

                var person = UserHelper.GetCurentPerson(this.db);

                if (state == OrderState.Canceled)
                    if (order.Person.Id == person.Id)
                    {
                        order.State = OrderState.Canceled;
                        this.db.HookahOrders.AddOrUpdate(order);
                        await this.db.SaveChangesAsync();
                        return this.Json(new { success = true });
                    }

                if (order.Place.Managers.Contains(person))
                {
                    order.State = state;
                    this.db.HookahOrders.AddOrUpdate(order);
                    await this.db.SaveChangesAsync();
                    return this.Json(new { success = true });
                }
            }
            catch (Exception e)
            {
                return this.Json(new { success = false, msg = e });
            }

            return this.Json(new { success = false, msg = string.Empty });
        }

        private async Task<Address> GetLocation(Address address)
        {
            var key = ConfigurationManager.AppSettings["googleMapApiKey"];
            var result = await GoogleGeocodingAPI.SearchAddressAsync(address.ToString());

            if (result.Results.Any())
            {
                var firstResult = result.Results.First();
                address.Lat = firstResult.Geometry.Location.Lat.ToString(CultureInfo.InvariantCulture);
                address.Lng = firstResult.Geometry.Location.Lng.ToString(CultureInfo.InvariantCulture);
                address.Location = DbGeography.FromText($"POINT({address.Lat} {address.Lng})");
            }

            return address;
        }
    }

    public class GetStatisticModel
    {
        public PlotData dayDistribution { get; set; }

        public PlotData timeDistribution { get; set; }

        public PlotData groupSize { get; set; }

        public PlotData visitDuration { get; set; }

        public PlotData weekVisits { get; set; }

        public PlotData monthVisit { get; set; }

        public int customers { get; set; }

        public string Start { get; set; }

        public string DataSpan { get; set; }

        public int ReservationCount { get; set; }

        public PlotData tableUssage { get; set; }
    }

    public class PlotData
    {
        public PlotData(List<string> labels, List<int> data)
        {
            this.Labels = labels;
            this.Data = data;
        }

        public List<string> Labels { get; set; }

        public List<int> Data { get; set; }
    }
}