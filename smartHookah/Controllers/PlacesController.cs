namespace smartHookah.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Resources;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;

    using GuigleAPI;

    using smartHookah.Helpers;
    using smartHookah.Models;
    using smartHookah.Models.Redis;

    using smartHookahCommon;

    /// <summary>
    /// The places controller.
    /// </summary>
    public class PlacesController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly SmartHookahContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlacesController"/> class.
        /// </summary>
        /// <param name="db">
        /// The db.
        /// </param>
        public PlacesController(SmartHookahContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var model = new PlaceIndexViewModel();

            var person = UserHelper.GetCurentPerson(db);
            model.Places = db.Places.ToList();
            if (person != null)
            {
                var ownPlace = model.Places.FirstOrDefault(a => a.Managers.Contains(person));

                if (ownPlace != null)
                {
                    model.ownPlaceId = ownPlace.Id;
                }
            }
            return this.View(model);
        }

        /// <summary>
        /// The order details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> OrderDetails(int id)
        {
            var order = await this.db.HookahOrders.FindAsync(id);

            var model = new OrderDetailsViewModel { order = order };
            return this.View(model);
        }

        /// <summary>
        /// The process order.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<ActionResult> ProcessOrder(int id)
        {
            var order = await this.db.HookahOrders.FindAsync(id);

            var model = new ProcessOrderViewModel();
            var person = order.Place.Person;
            model.SmokeMetadataModalViewModel =
                SmokeMetadataModalViewModel.CreateSmokeMetadataModalViewModel(this.db, order.SmokeSessionMetaData, person);
            model.order = order;

            var hookahs = person.Hookahs.ToList();
            
            var hookahSessionId = RedisHelper.GetSmokeSessionIds(hookahs.Select(a => a.Code).ToList());
            model.Hookahs = new List<Hookah>();
            foreach (var sessionId in hookahSessionId)
            {
                var session = this.db.SmokeSessions.Include(a => a.Pufs).FirstOrDefault(a => a.SessionId == sessionId);

                var pufs = RedisHelper.GetPufs(sessionId);

                if (session == null)
                    continue;

                if (!pufs.Any())
                    model.Hookahs.Add(session.Hookah);
            }
            model.Seats = order.Place.Seats.ToList();
            return this.View(model);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> ProcessOrder(int id, int hookahId,int?seatId,int? reservationId)
        {
            var order = await db.HookahOrders.FindAsync(id);
            var hookah = await db.Hookahs.FindAsync(hookahId);

            if (order == null || hookah == null)
                return null;

            var sessionId = RedisHelper.GetSmokeSessionId(hookah.Code);

            var session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);

            if (seatId != null || seatId != 0 )
            {
                order.SeatId = seatId;
             
            }

            if (seatId != null || seatId != 0)
            {
                order.ReservationId = reservationId;

            }

            session.MetaData = order.SmokeSessionMetaData;
            order.SmokeSession = session;

            session.Persons.Add(order.Person);
            order.State = OrderState.Ready;

            db.SmokeSessions.AddOrUpdate(session);
            db.HookahOrders.AddOrUpdate(order);

            await db.SaveChangesAsync();


            return RedirectToAction("SmokeSession", "SmokeSession", new {id = session.SessionId});
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> PostOrder(int id, PostOrderModel model)
        {
            var smokeSessionMetadata = new SmokeSessionMetaData();
            var place = await db.Places.FindAsync(id);
            if (model.Tobacco.Any(a => a != 0))
            {
                var metadataModel = new SaveSmokeMetadataModel();
                metadataModel.TobacosMix = new List<TobacoMixPart>();
                foreach (var i in model.Tobacco)
                    metadataModel.TobacosMix.Add(new TobacoMixPart
                    {
                        Id = 0,
                        TobaccoId = i,
                        Fraction = 5
                    });

                var tobacco = TobaccoController.GetTobacoFromMetadata(metadataModel, db);
                smokeSessionMetadata.Tobacco = tobacco;
            }

            if (model.Hookah.Any(a => a != 0))
                smokeSessionMetadata.PipeId = model.Hookah.First();

            if (model.Bowl.Any(a => a != 0))
                smokeSessionMetadata.BowlId = model.Bowl.First();


            var person = UserHelper.GetCurentPerson(db);

           

            var descriptionSb = new StringBuilder();
            descriptionSb.Append("Extra:\n");

            if (model.Extra != null && model.Extra.Any())
            {
                var orderExtra = db.OrderExtras.Where(a => model.Extra.Contains(a.Id));

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
            var order =
                new HookahOrder
                {
                    Created = DateTime.Now.ToUniversalTime(),
                    ExtraInfo = descriptionSb.ToString(),
                    SmokeSessionMetaData = smokeSessionMetadata,
                    Person = person,
                    State = OrderState.Open,
                    Place = place,
                    
                };

            if (model.Seat != 0)
                order.SeatId = model.Seat;

            try
            {
                db.SessionMetaDatas.Add(smokeSessionMetadata);
                db.HookahOrders.Add(order);
                await db.SaveChangesAsync();


                return Json(new {success = true, order = order.Id});
            }
            catch (Exception e)
            {
                return Json(new {success = false, Error = e.Message});
            }
        }

        // GET: Places/test
        public ActionResult Details(string id)
        {
            var lounge = db.Places.Where(a => a.FriendlyUrl == id).Include(a => a.Person)
                .Include(a => a.Person.OwnedPipeAccesories).FirstOrDefault();
            var model = new PlaceDetailsViewModel();
            if (lounge == null)
                return HttpNotFound();

            var person = UserHelper.GetCurentPerson(db, lounge.PersonId);
            model.Place = lounge;
            model.CanEdit = person != null;
            return View(model);
        }

        public ActionResult Media(int id)
        {
            Place place;
            Person person;
            var canManage = canManagePlace(out place, id, out person);

            if (canManage)
            {
                return View(place);
            }
           return RedirectToAction("Index");
        }

        private bool canManagePlace(out Place place, int id, out Person person)
        {
            place = db.Places.Find(id);

            person = UserHelper.GetCurentPerson(db);
            var personId = person.Id;
            var canManage = place.Managers.Any(a => a.Id == personId);
            return canManage;
        }

        public ActionResult AddMedia(int id)
        {
            Place place;
            Person person;
            var canManage = canManagePlace(out place, id, out person);

            if (canManage)
            {
                return View(place);
            }
            return RedirectToAction("Index");
        }



        public async Task<ActionResult> Manage(int? id, int? personId)
        {
            var person = UserHelper.GetCurentPerson(db, personId);

            if (id == null)
            {
                if (person == null)
                    return RedirectToAction("Index");

                var managedPlaces = db.Places.Where(a => a.Managers.Any(p => p.Id == person.Id));

                if (managedPlaces.Count() == 1)
                    id = managedPlaces.FirstOrDefault().Id;
                else
                    return View("ManagedPlaces", managedPlaces.ToList());
            }

            var place = db.Places.Include(a => a.Orders).FirstOrDefault(a => a.Id == id);

            if (place == null)
                return RedirectToAction("Index");


            if (person == null || !place.Managers.Contains(person))
                return RedirectToAction("Index");

            return View(place);
        }

        // GET: Places/Details/5
        public ActionResult DetailsById(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var place = db.Places.Find(id);
            if (place == null)
                return HttpNotFound();

            var model = new PlaceDetailsViewModel {Place = place};

            return View("Details", model);
        }

        //GET Places/EditOpenHours/1
        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> EditOpenHours(int? id)
        {
            var model = new EditOpenHoursViewModel();
            var place = db.Places.Include(b => b.BusinessHours).FirstOrDefault(a => a.Id == id);

            if (place == null)
                return RedirectToAction("Index", "Places");

            var person = UserHelper.GetCurentPerson(db, place.Person.Id);

            if (person == null)
                return RedirectToAction("Index", "Places");

            model.BusinessHours = place.BusinessHours.ToList();

            for (var i = 0; i < 7; i++)
                if (model.BusinessHours.All(a => a.Day != i))
                    model.BusinessHours.Add(new BusinessHours
                    {
                        Day = i,
                        OpenTine = new TimeSpan(),
                        CloseTime = new TimeSpan()
                    });
            model.Place = place;
            return View(model);
        }

        [System.Web.Mvc.Authorize]
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOpenHours(EditOpenHoursPostModel model)
        {
            var place = db.Places.Include(d => d.BusinessHours).FirstOrDefault(a => a.Id == model.Id);

            if (place == null)
                return null;

            var person = UserHelper.GetCurentPerson(db, place.Person.Id);
            if (person == null)
                return RedirectToAction("Index", "Places");
            foreach (var hour in model.BusinessHours)
            {
                if (string.IsNullOrEmpty(hour.Open) || string.IsNullOrEmpty(hour.Close))
                    continue;
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

                db.BusinessHours.AddOrUpdate(bh);
            }

            db.SaveChanges();

            return RedirectToAction("Details", "Places", new {id = place.FriendlyUrl});
        }

        public ActionResult EditPrice(int id)
        {
            var place = db.Places.Include(a => a.Person.OwnedPipeAccesories).FirstOrDefault(a => a.Id == id);

            if (place == null)
                return RedirectToAction("Index", "Places");

            var model = new EditPriceViewModel();
            model.OwnedGear = place.Person.OwnedPipeAccesories.OrderBy(a => a.PipeAccesory.BrandName)
                .ThenBy(a => a.PipeAccesory.AccName).ToList();
            model.Extras = place.OrderExtras.ToList();
            model.PlaceId = place.Id;
            model.BasePrice = place.BaseHookahPrice;
            model.Currency = place.Currency;
            model.PriceGroups = place.PriceGroups.ToList();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> EditPrice(EditPricePostModel model)
        {
            var place = db.Places.Include(a => a.Person.OwnedPipeAccesories).FirstOrDefault(a => a.Id == model.PlaceId);

            if (place == null)
                return RedirectToAction("Index", "Places");

            if (place.Currency != model.Currency)
            {
                place.Currency = model.Currency;
                this.db.Places.AddOrUpdate(place);
            }

            foreach (var placePriceGroup in place.PriceGroups)
            {
                var newPriceGroup = model.priceGroup.FirstOrDefault(a => a.Id == placePriceGroup.Id && a.PriceValue != placePriceGroup.Price);
                if(newPriceGroup == null)
                    continue;

                placePriceGroup.Price = newPriceGroup.PriceValue;
                this.db.PriceGroup.AddOrUpdate(placePriceGroup);

            }

            var gear = place.Person.OwnedPipeAccesories.ToList();
            foreach (var item in model.Items)
            {
                var gearItem = gear.FirstOrDefault(a => a.Id == item.Id);
                if (gearItem == null)
                    continue;

                foreach (var price in item.Prices)
                {
                    if(gearItem.Prices == null)
                        gearItem.Prices = new List<PriceGroupPrice>();
                    var priceGroup = gearItem.Prices.FirstOrDefault(a => a.PriceGroupId == price.Id);
                    if (priceGroup == null)
                    {
                        priceGroup = new PriceGroupPrice();
                        priceGroup.PriceGroupId = price.Id;
                        priceGroup.OwnPipeAccesoriesId = gearItem.Id;
                        gearItem.Prices.Add(priceGroup);
                        priceGroup.Price = price.PriceValue;
                        db.PriceGroupPrice.AddOrUpdate(priceGroup);
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

                //db.OwnPipeAccesorieses.AddOrUpdate(gearItem);
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Details", new {id = place.FriendlyUrl});
        }

        private static TimeSpan GetTimeSpan(string text)
        {
            var dateTime = DateTime.ParseExact(text,
                "h:mm tt", CultureInfo.InvariantCulture);
            var span = dateTime.TimeOfDay;
            return span;
        }


        [System.Web.Mvc.Authorize]
        // GET: Lounge/Create
        public ActionResult Create()
        {
            return View();
        }


        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> Create(
            [Bind(Include = "Id,Name,FriendlyUrl,LogoPath,Descriptions,ShortDescriptions,Address,PhoneNumber,Facebook")]
            Place lounge,
            HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var path = $"/Content/PlacePictures/";
                    var extension = Path.GetExtension(file.FileName);
                    lounge.LogoPath = path + lounge.FriendlyUrl + extension;
                    lounge.Address = await GetLocation(lounge.Address);
                    file.SaveAs(Server.MapPath(lounge.LogoPath));
                }


                db.Places.Add(lounge);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lounge);
        }

        public async Task<ActionResult> Menu(string id)
        {
            return await Order(id, null, null);
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
            }

            return address;
        }

        public ActionResult DashBoard(int? id)
        {
            return View(id);
        }

        public ActionResult PublicDashBoard(int? id)
        {
            return View(id);
        }

        [System.Web.Mvc.Authorize]
        public async Task<JsonResult> DashBoardData(int? id)
        {
            var place = db.Places.Where(a => a.Id == id).Include(a => a.Person).Include(a => a.Person.Hookahs)
                .FirstOrDefault();

            var model = new LoungeDashBoardViewModel();

            if (place == null)
                return Json(model, JsonRequestBehavior.AllowGet);

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
                        estTobaco = sessionMetadata.MetaData.Tobacco.GetTobacoEstimated(db);
                    if (sessionMetadata.MetaData.Pipe != null && sessionMetadata.MetaData.Pipe.Picture != null)
                        hookahPicture = sessionMetadata.MetaData.Pipe.Picture;
                }


                DynamicSmokeStatistic dynamicOut;
                if (!DynamicStatistic.TryGetValue(hookah.Code, out dynamicOut))
                    dynamicOut = new DynamicSmokeStatistic();
                model.Hookah.Add(new HookahDashboardViewModel
                {
                    Key = hookah.Code,
                    Name = hookah.Name,
                    DynamicSmokeStatistic = new ClientDynamicSmokeStatistic(dynamicOut),
                    Online = onlineHookah.Contains(hookah.Code),
                    EstPufCount = (int) estTobaco,
                    HookahPicture = hookahPicture,
                    Table = RedisHelper.GetHookahSeat(hookah.Code)
                });
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [System.Web.Mvc.Authorize]
        public async Task<JsonResult> GetOnlineState(int? id)
        {
            var place = db.Places.Where(a => a.Id == id).Include(a => a.Person).Include(a => a.Person.Hookahs)
                .FirstOrDefault();


            if (place == null)
                return Json(JsonRequestBehavior.AllowGet);

            var hookahs = place.Person.Hookahs.Select(a => a.Code).ToList();

            var hookahState = new Dictionary<string, string>();


            var online = await IotDeviceHelper.GetState(hookahs);
            var offline = hookahs.Except(online).ToList();

            return Json(new {online, offline}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OrderSpecific(string id)
        {
            if (string.IsNullOrEmpty(id))
                RedirectToAction("Index");

            var model = new OrderSpecificModel();

            var place = db.Places.FirstOrDefault(a => a.FriendlyUrl == id);

            return View(place);
        }

        [System.Web.Mvc.HttpGet]
        public async Task<ActionResult> Order(string id, int? resId = null,string seatId = null )
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            //if (resId == null)
            //{
            //    return RedirectToAction("OrderSpecific", new {id = id});
            //}

            var place = db.Places.Include(a => a.Person).Include(a => a.Person.OwnedPipeAccesories)
                .Include(a => a.Seats)
                .FirstOrDefault(a => a.FriendlyUrl == id);

            if (place == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);


            var model = new OrderHookahViewModel
            {
                Place = place,
                Hookah = place.Person.Pipes,
                Bowls = place.Person.Bowls,
                Tobacco = place.Person.Tobacco
            };

            if (!string.IsNullOrEmpty(seatId))
            {
                resId = RedisHelper.GetReservatopnFromTable(seatId);
            }


            if (resId.HasValue)
            {
                var reservation = db.Reservations.Find(resId);
                if (reservation != null && reservation.Seats != null)
                    model.Seat = reservation.Seats;

                if (reservation != null)
                {
                    model.Reservation = reservation;
                }
            }

            if (resId == null)
                model.CanOrder = false;
      
            return View("Order",model);
        }

  

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> CancelOrder(int id)
        {
            return await ChangeOrderState(id, OrderState.Canceled);
        }

        [System.Web.Mvc.HttpPost]
        public async Task<JsonResult> ChangeOrderState(int id, int state)
        {
            return await ChangeOrderState(id, (OrderState) state);
        }

        private async Task<JsonResult> ChangeOrderState(int id, OrderState state)
        {
            try
            {
                var order = await db.HookahOrders.FindAsync(id);

                if (order == null)
                    return Json(new {success = false, msg = "Order not found"});

                var person = UserHelper.GetCurentPerson(db);

                if (state == OrderState.Canceled)
                    if (order.Person.Id == person.Id)
                    {
                        order.State = OrderState.Canceled;
                        db.HookahOrders.AddOrUpdate(order);
                        await db.SaveChangesAsync();
                        return Json(new {success = true});
                    }

                if (order.Place.Managers.Contains(person))
                {
                    order.State = state;
                    db.HookahOrders.AddOrUpdate(order);
                    await db.SaveChangesAsync();
                    return Json(new {success = true});
                }
            }
            catch (Exception e)
            {
                return Json(new {success = false, msg = e});
            }

            return Json(new {success = false, msg = ""});
        }

        public async Task<ActionResult> Seat(int? id)
        {
            var place = db.Places.Include(a => a.Person).Include(a => a.Seats)
                .FirstOrDefault(a => a.Id == id);

            if (place == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new SeatManagerModel();
            model.Seats = place.Seats.ToList();
            model.PlaceId = id.Value;
            model.place = place;
            return View(model);
        }

        public async Task<ActionResult> EditSeat(int? id)
        {
            var place = db.Places.Include(a => a.Person).Include(a => a.Seats)
                .FirstOrDefault(a => a.Id == id);

            if (place == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            var model = new SeatManagerModel();
            model.Seats = place.Seats.ToList();
            model.PlaceId = id.Value;
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.ActionName("EditSeat")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSeat(int id, [FromBody] List<Seat> seat)
        {
            var place = db.Places.Include(a => a.Person).Include(a => a.Seats)
                .FirstOrDefault(a => a.Id == id);


            foreach (var s in seat.Where(a => a.Id != 0))
            {
                var placeSeat = place.Seats.FirstOrDefault(a => a.Id == s.Id);

                if (placeSeat == null)
                    continue;

                if (placeSeat.Name == s.Name && placeSeat.Capacity == s.Capacity) continue;

                if (placeSeat.Name != s.Name)
                {
                    placeSeat.Name = s.Name;
                }


                if (placeSeat.Capacity != s.Capacity)
                {
                    placeSeat.Capacity = s.Capacity;
                }
                  

                db.Seats.AddOrUpdate(placeSeat);
            }

            foreach (var s in seat.Where(a => a.Id == 0))
            {
                s.Place = place;
                s.PlaceId = place.Id;
                s.Code = Support.Support.RandomString(5);
                db.Seats.Add(s);
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Seat",new {id = id});
        }


        public async Task<JsonResult> GetPlaceMenu(int? id, bool canOrder = false)
        {
            var place = db.Places.Include(a => a.Person).Include(a => a.Person.OwnedPipeAccesories)
                .FirstOrDefault(a => a.Id == id);

            var model = new GetPlaceMenuModel();

            if (place != null)
            {
                model.BasePrice = place.BaseHookahPrice;
                model.Hookah = place.Person.OwnedPipeAccesories
                    .Where(a => a.PipeAccesory is Pipe && a.DeleteDate == null).Select(a => new AccesoryDto(a))
                    .ToList();
                if (canOrder)
                    model.Hookah.Insert(0, AccesoryDto.GetDefault());
                model.Bowl = place.Person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Bowl && a.DeleteDate == null)
                    .Select(a => new AccesoryDto(a)).ToList();
                if (canOrder)
                    model.Bowl.Insert(0, AccesoryDto.GetDefault());
                model.Tobacco = place.Person.OwnedPipeAccesories
                    .Where(a => a.PipeAccesory is Tobacco && a.DeleteDate == null && String.IsNullOrEmpty(a.AlternativeName)).Select(a => new AccesoryDto(a))
                    .ToList();

                model.AlternativeTobacco = place.Person.OwnedPipeAccesories
                    .Where(a => a.PipeAccesory is Tobacco && a.DeleteDate == null && !String.IsNullOrEmpty(a.AlternativeName)).Select(a => new AccesoryDto(a))
                    .ToList();

                if (canOrder)
                model.Tobacco.Insert(0, AccesoryDto.GetDefault());

                model.Mixes = db.TobaccoMixs.Where(a => a.AuthorId == place.PersonId && a.AccName != null).ToList()
                    .Select(a => new AccesoryDto(a))
                    .ToList();

                model.Extra = db.OrderExtras.Where(a => a.PlaceId == id).ToList().Select(a => new OrderExtraDto(a))
                    .ToList();

                model.PriceGroup = place.PriceGroups.ToList().Select(a => new PriceGroupDto(a)).ToList();

                model.PriceMatrix = new Dictionary<string, Dictionary<string, decimal>>();

                foreach (var pc in model.PriceGroup)
                {
                 
                    var pcMatrix = new Dictionary<string, decimal>();
                        foreach (var item in place.Person.OwnedPipeAccesories)
                        {
                            var priceGroup = item.Prices.FirstOrDefault(a => a.PriceGroupId == pc.Id);
                        if (priceGroup != null)
                            pcMatrix.Add(item.PipeAccesoryId.ToString(),priceGroup.Price);
                    }
                        model.PriceMatrix.Add(pc.Id.ToString(),pcMatrix);
                }

                AddDefautlImgPath(model.Bowl, "/Content/icons/bowl.svg");
                AddDefautlImgPath(model.Hookah, "/Content/icons/hookah.svg");
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private void AddDefautlImgPath(List<AccesoryDto> accesory, string path)
        {
            foreach (var acc in accesory)
                if (acc.Picture == null)
                    acc.Picture = path;
        }

        // GET: Lounge/Edit/5
        [System.Web.Mvc.Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var lounge = db.Places.Find(id);
            if (lounge == null)
                return HttpNotFound();
            return View(lounge);
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
            if (ModelState.IsValid)
            {
                db.Entry(lounge).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lounge);
        }

        // GET: Lounge/Delete/5
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var lounge = db.Places.Find(id);
            if (lounge == null)
                return HttpNotFound();
            return View(lounge);
        }

        // POST: Lounge/Delete/5
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var lounge = db.Places.Find(id);
            db.Places.Remove(lounge);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }

        public class AccesoryDto
        {
            public AccesoryDto()
            {
            }

            public AccesoryDto(PipeAccesory a)
            {
                Name = a.AccName;
                Brand = a.Brand.DisplayName;
                Id = a.Id;
                Picture = a.Picture;
            }

            public AccesoryDto(OwnPipeAccesories a) : this(a.PipeAccesory)
            {
                if (a.Price != null)
                {
                    Price = a.Price;
                    Currency = a.Currency;
                }
            }

            public string Name { get; set; }
            public string Brand { get; set; }
            public int Id { get; set; }
            public string Picture { get; set; }

            public decimal? Price { get; set; }

            public string Currency { get; set; }

            public static AccesoryDto GetDefault(string picturePath = null)
            {
                var rm = new ResourceManager("smartHookah.Resources.Order.Order", typeof(Order.Order).Assembly);
                return new AccesoryDto
                {
                    Name = rm.GetString("letusChoose"),
                    Brand = "",
                    Id = 0,
                    Picture = picturePath
                };
            }
        }

        [System.Web.Http.HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Http.ActionName("AddMedia")]
        public async Task<ActionResult> AddMediaPost(int id, HttpPostedFileBase file)
        {
            var place = await db.Places.FindAsync(id);
            var path = $"/Content/Place/";
            if (file != null)
            {
                var media = new Media();
                var lastId = place.Medias.OrderBy(a => a.Id).Select(a => a.Id).DefaultIfEmpty(0).First()  +1 ;
                var extension = System.IO.Path.GetExtension(file.FileName);
                var scalePath = path + place.FriendlyUrl + "/" + lastId;
                media.Path = scalePath + extension;
                media.Created = DateTime.Now;    
                System.IO.Directory.CreateDirectory(Server.MapPath(path + place.FriendlyUrl));

                System.Drawing.Image sourceimage =
                    System.Drawing.Image.FromStream(file.InputStream);
                MediaController.ScaleAndSave(sourceimage,180,200, scalePath, Server);

                file.SaveAs(Server.MapPath(media.Path));
                place.Medias.Add(media);
                db.Places.AddOrUpdate(place);
                await db.SaveChangesAsync();
            }


            return RedirectToAction("Media", "Places",new {id = place.Id});
        }

        public ActionResult DeleteMedia(int id, int mediaId)
        {
            Place place;
            Person person;
            var canManage = canManagePlace(out place, id, out person);

            if (canManage)
            {
                var media = db.Media.Find(mediaId);
                var serverPath = Server.MapPath(media.GetDirectory);
                var dir = new DirectoryInfo(serverPath);
               
                foreach (var file in dir.EnumerateFiles($"{media.FileName}*"))
                {
                    file.Delete();
                }

                db.Media.Remove(media);
                db.SaveChanges();

                return RedirectToAction("Media", new {id = place.Id});


            }
            return RedirectToAction("Index");
        }
    }

    public class PriceGroupDto
    {
        public string Name { get; set; }

        public int Id { get;set; }

        public decimal Price { get; set; }
        public PriceGroupDto(PriceGroup priceGroup)
        {
            this.Name = priceGroup.Name;
            this.Id = priceGroup.Id;
            this.Price = priceGroup.Price;
        }
    }

    public class OrderSpecificModel
    {
        public Place place { get; set; }
    }
}