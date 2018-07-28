﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using EntityFramework.BulkInsert.Extensions;
using Microsoft.ApplicationInsights;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using smartHookah.Helpers;
using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using smartHookahCommon;
using static System.Threading.Tasks.Task;

namespace smartHookah.Controllers
{
    using smartHookah.Services.Person;

    [System.Web.Mvc.Authorize]
    
    public class SmokeSessionController : Controller
    {
        private readonly SmartHookahContext _db;
        private readonly IHubContext hubContext;

        private readonly IPersonService personService;

        private TelemetryClient telemetry = new TelemetryClient();

        private IRedisService redisService;

        public SmokeSessionController(SmartHookahContext db, IRedisService redisService, IPersonService personService)
        {
            _db = db;
            this.redisService = redisService;
            this.personService = personService;
        }

        public SmokeSessionController()
        {
            hubContext = GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        }

        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        public async Task<ActionResult> EndSmokeSession(string id)
        {
            var sessionId = await EndSmokeSession(id, _db);

            if (sessionId == 0)
                return RedirectToAction("Index", "Home");

            return RedirectToAction("GetStatistics", new {id = sessionId});
        }

        // End smoke session
        public static async Task<int> EndSmokeSession(string id, SmartHookahContext db, bool autoEnd = false)
        {
            // Get hookah code from session Id
            var hookahId = RedisHelper.GetHookahId(id);
            var rawPufs = RedisHelper.GetPufs(id);

            if (!rawPufs.Any())
                return 0;

            var smokeSession = new SmokeSession();

            var dbSmokeSession = db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            if (dbSmokeSession != null)
                smokeSession = dbSmokeSession;

            smokeSession.SessionId = id;
            var hookahCode = RedisHelper.GetHookahId(id);
            smokeSession.Hookah = db.Hookahs.FirstOrDefault(a => a.Code == hookahCode);
            smokeSession.Statistics = new SmokeSessionStatistics(rawPufs);

            smokeSession.Report = autoEnd ? SessionReport.AutomaticEnd : SessionReport.Good;

            // ReSharper disable once UseNullPropagation
            if (smokeSession.Hookah != null)
            {
                var defaultAnimation = smokeSession.Hookah.Owners.Where(a => a.DefaultSetting != null)
                    .Select(a => a.DefaultSetting).FirstOrDefault();
                if (defaultAnimation != null)
                {
                    smokeSession.Hookah.Setting.Change(defaultAnimation);
                    db.HookahSettings.AddOrUpdate(smokeSession.Hookah.Setting);
                }
            }

            db.SmokeSessions.AddOrUpdate(smokeSession);
            db.SaveChanges();

            var pufs = rawPufs.Select(a => new DbPuf(a) {SmokeSession_Id = smokeSession.Id}).ToList();
            var options = new BulkInsertOptions {BatchSize = 1000};
            // Default is 5000
            db.BulkInsert(pufs, options);


            db.SaveChanges();
            RedisHelper.EndSmokeSession(id);
            if (smokeSession.MetaData != null && smokeSession.MetaData.TobaccoId.HasValue)
                TobaccoController.CalculateStatistic(smokeSession.MetaData.TobaccoId.Value, db);


            await IotDeviceHelper.SendMsgToDevice(hookahId, "restart:");

            GamificationController.Engine.ManualValidateSession(smokeSession.Id);


            return smokeSession.Id;
        }


        public void GetRawData(int id)
        {
            var dbSmokeSession = _db.SmokeSessions.Find(id);
            if (dbSmokeSession == null)
                return;
            var pufs = _db.DbPufs.Where(a => a.SmokeSessionId == dbSmokeSession.SessionId).ToList();

            var clusters = pufs.GetClusterPuf().ToArray();

            using (var wb = new XLWorkbook())
            {
                var metaData = wb.Worksheets.Add("MetaData");
                var data = wb.Worksheets.Add("Raw");

                metaData.Cell("A1").Value = "Hookah";
                if (dbSmokeSession.MetaData.Pipe != null)
                    metaData.Cell("B1").Value =
                        $"{dbSmokeSession.MetaData.Pipe.Brand.Name} {dbSmokeSession.MetaData.Pipe.AccName}";


                metaData.Cell("A2").Value = "Bowl";
                if (dbSmokeSession.MetaData.Bowl != null)
                    metaData.Cell("B2").Value =
                        $"{dbSmokeSession.MetaData.Bowl.Brand.Name} {dbSmokeSession.MetaData.Bowl.AccName}";


                metaData.Cell("A3").Value = "TobaccoSimple";
                if (dbSmokeSession.MetaData.Tobacco != null)
                    metaData.Cell("B3").Value =
                        $"{dbSmokeSession.MetaData.Tobacco.Brand.Name} {dbSmokeSession.MetaData.Tobacco.AccName}";


                data.Cell("A1").Value = "Time";
                data.Cell("B1").Value = "Presure";
                data.Cell("C1").Value = "TimeStamp";

                for (var i = 0; i < clusters.Count(); i++)
                {
                    var c = clusters[i];
                    data.Cell($"A{i + 2}").Value = c.Duration.TotalMilliseconds;
                    data.Cell($"B{i + 2}").Value = c.Presure;
                    data.Cell($"C{i + 2}").Value = c.TimeStamp.ToString("HH:mm:ss.fff");
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={dbSmokeSession.SessionId}.xlsx");
                using (var myMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public void CreateSmokeStats(int id)
        {
            var dbSmokeSession = _db.SmokeSessions.Find(id);
            if (dbSmokeSession == null)
                return;
            var pufs = _db.DbPufs.Where(a => a.SmokeSessionId == dbSmokeSession.SessionId).ToList();

            dbSmokeSession.Statistics = new SmokeSessionStatistics(pufs);

            _db.SmokeSessions.AddOrUpdate(dbSmokeSession);
            _db.SaveChanges();
        }

        public ActionResult RecalculateSmokeStats(int id)
        {
            CreateSmokeStats(id);
            return RedirectToAction("GetStatistics", id);
        }

        public void CreateAllStats()
        {
            var ss = _db.SmokeSessions.Where(a => a.Statistics == null).ToList();

            foreach (var smokeSession in ss)
            {
                if (RedisHelper.GetHookahId(smokeSession.SessionId) != null)
                    continue;

                CreateSmokeStats(smokeSession.Id);
            }
        }


        public ActionResult EndSmokeSessionSilent(string id)
        {
            RedisHelper.EndSmokeSession(id);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Index(int id)
        {
            var model = new SmokeSessionViewModel();
            model.SmokeSessions = _db.SmokeSessions.Where(a => a.HookahId == id)
                .Include(x => x.MetaData.Pipe)
                .Include(x => x.MetaData.Tobacco)
                .Include(x => x.MetaData.Pipe.Brand)
                .Include(x => x.MetaData.Tobacco.Brand)
                .Include(x => x.Statistics).OrderByDescending(a => a.Id).ToList();
            return View(model);
        }


        public ActionResult GetStatistics(int id, int? PersonId = null)
        {
            var model = new SmokeSessionGetStaticsticsModel();


            model.Create(_db, id);

            if (PersonId.HasValue)
                model.Share = !model.SmokeSession.IsPersonAssign(PersonId.Value);

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult GetLiveSmokeStatics(string sessionId)
        {
            try
            {
                var model = new SmokeStatisticViewModel();

                var dynamic = DynamicSmokeStatistic.GetStatistic(sessionId);

                if (dynamic.LastPuf == null)
                    return PartialView("NoLiveStatistic");
                model.Start = dynamic.Start;
                model.CurentState = dynamic.LastPuf;
                model.LongestPuf = dynamic.LongestPuf;
                model.LastPufDuration = dynamic.LastPufDuration;
                model.LastPufTime = dynamic.LastPufTime;
                model.PufCount = dynamic.PufCount;
                model.Duration = dynamic.TotalSmokeTime;

                return PartialView(model);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ActionResult> DeleteSmokeStatistic(string id)
        {
            var intId = 0;
            if (!int.TryParse(id, out intId))
                return RedirectToAction("SmokeSession", "SmokeSession", new {id});
            var ss = await _db.SmokeSessions.FindAsync(intId);
            var hookah = ss.Hookah.Id;
            _db.DbPufs.RemoveRange(ss.Pufs);
            _db.SessionMetaDatas.Remove(ss.MetaData);
            if (ss.Statistics != null)
                _db.SessionStatistics.Remove(ss.Statistics);
            _db.SmokeSessions.Remove(ss);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "SmokeSession", new {id = hookah});
        }


        public static SmokeSession InitSmokeSession(SmartHookahContext db, string deviceId = null,
            string sessionId = null)
        {
            var newSessionId = "";
            if (string.IsNullOrEmpty(sessionId))
                newSessionId = RedisHelper.GetSmokeSessionId(deviceId);
            else
                newSessionId = sessionId;


            var session = db.SmokeSessions.FirstOrDefault(s => s.SessionId == newSessionId);


            if (session != null) return session;

            var dbSession = new SmokeSession();
            dbSession.SessionId = newSessionId;

            dbSession.MetaData = new SmokeSessionMetaData();
            dbSession.Token = Support.Support.RandomString(10);
            var hookah = db.Hookahs.Where(a => a.Code == deviceId).Include(a => a.Owners).FirstOrDefault();

            if (hookah != null)
            {
                dbSession.Hookah = hookah;

                if (hookah.DefaultMetaData != null)
                    dbSession.MetaData.Copy(hookah.DefaultMetaData);

                foreach (var hookahOwner in hookah.Owners.Where(a => a.AutoAssign))
                {
                    if (hookahOwner.SmokeSessions.Any(a => a.SessionId == dbSession.SessionId))
                        continue;

                    hookahOwner.SmokeSessions.Add(dbSession);
                    db.Persons.AddOrUpdate(hookahOwner);
                }

                foreach (var placeOwner in hookah.Owners.Where(a => a.Place != null))
                    dbSession.PlaceId = placeOwner.Place.Id;
            }

            db.SmokeSessions.Add(dbSession);
            db.SaveChanges();
            return dbSession;
        }

        public void InitOldSmokeSession(int hookahId, string sessionId)
        {
            using (var db = new SmartHookahContext())
            {
                var dbSession = db.SmokeSessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (dbSession.MetaData != null)
                    return;

                dbSession.SessionId = sessionId;
                dbSession.MetaData = new SmokeSessionMetaData();

                var hookah = db.Hookahs.Find(hookahId);
                if (hookah != null)
                    dbSession.Hookah = hookah;

                db.SmokeSessions.AddOrUpdate(dbSession);
                db.SaveChanges();
            }
        }

        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> Hookah(string id)
        {
            var model = new SmokeViewModel();
            await model.CreateModel(_db, reddisHookahId: id);

            await AssignToSmokeSession(model.Session, true);
            return View("Smoke", model);
        }


        [AllowAnonymous]
        public async Task<ActionResult> SmokeSession(string id)
        {
            var model = new SmokeViewModel();

            var session = _db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);

            if (session == null)
                RedirectToAction("Index", "Home");

            if (session?.StatisticsId != null)
            {
                var statmodel = new SmokeSessionGetStaticsticsModel();
                statmodel.Create(_db, session.Id);
                await AssignToSmokeSession(session, true);
                return View("GetStatistics", statmodel);
            }

            await model.CreateModel(_db, redisSessionId: id);

            return View("Smoke", model);
        }

        private async Task<bool> AssignToSmokeSession(SmokeSession session, bool assign, bool manual = false)
        {
            if (session == null)
                return false;

            var person = this.personService.GetCurentPerson();

            if (person == null)
                return false;

            if (User.IsInRole("Admin") && !manual)
                return false;

            if (assign)
            {
                if (session.IsPersonAssign(person.Id))
                    return false;

                session.Persons.Add(person);

                _db.SmokeSessions.AddOrUpdate(session);
                await _db.SaveChangesAsync();

                return true;
            }
            if (!session.IsPersonAssign(person.Id))
                return false;

            session.Persons.Remove(person);

            _db.SmokeSessions.AddOrUpdate(session);
            await _db.SaveChangesAsync();

            return true;
        }


        public Task<JsonResult> AddPersonToSmokeSession(string id)
        {
            var session = _db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            if (session == null)
                return null;


            var user =
                System.Web.HttpContext.Current.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (user.Person.SmokeSessions.FirstOrDefault(a => a.Id == session.Id) != null)
                return null;
            var person = _db.Persons.Find(user.PersonId);
            person?.SmokeSessions.Add(session);
            _db.SaveChanges();

            return null;
        }

        [HttpPost]
        public async Task<ViewResult> SaveSmokeMetadata(SaveSmokeMetadataModel model)
        {
            var dbSession = await _db.SmokeSessions.FindAsync(model.DbSmokeSessionId);

            if (dbSession == null)
            {
                var metadata = _db.SessionMetaDatas.Find(model.MetaDataId);
                if (metadata != null)
                    dbSession = new SmokeSession
                    {
                        MetaData = metadata,
                        Pufs = new List<DbPuf>(),
                        Persons = new List<Person>()
                    };
            }

            if (dbSession == null)
                return null;

            var tobacco = TobaccoController.GetTobacoFromMetadata(model, _db);

            if (tobacco != null)
            {
                dbSession.MetaData.Tobacco = tobacco;
            }
            else
            {
                dbSession.MetaData.Tobacco = null;
                dbSession.MetaData.TobaccoId = null;
            }

            Factory.StartNew(() => { UpdateDashboardData(dbSession); });

            dbSession.MetaData.TobaccoWeight = model.TobacoWeight;


            SetMetadata(model, dbSession.MetaData);

            dbSession.MetaData.AnonymPeopleCount = model.PersonCount - dbSession.Persons.Count;

            //Online Session
            if (!dbSession.Pufs.Any() && dbSession.MetaData.Tobacco != null && dbSession.Hookah != null)
                await SentPercentageToDevice(dbSession.Hookah, dbSession.MetaData.Tobacco);

            _db.SessionMetaDatas.AddOrUpdate(dbSession.MetaData);
            try
            {
                _db.SaveChanges();
            }
            catch (Exception e)
            {
            }


            return View("smokeSessionMetaData", dbSession.MetaData);
        }

        private void UpdateDashboardData(SmokeSession dbSession)
        {
            var hookah = _db.Hookahs.Find(dbSession.HookahId);
            var hooakhPicture = "/Content/icons/hookah.svg";
            if (hookah == null || dbSession.MetaData.Tobacco == null)
                return;

            try
            {
                if (dbSession.MetaData?.Pipe?.Picture != null)
                    hooakhPicture = dbSession.MetaData.Pipe.Picture;

                var estimated = dbSession.MetaData.Tobacco.GetTobacoEstimated(_db);
                ClientContext.Clients.Group(dbSession.Hookah.Code)
                    .updateStats(hookah.Code, (int) estimated, hooakhPicture);
            }

            catch (Exception)
            {
                ClientContext.Clients.Group(dbSession.Hookah.Code).updateStats(hookah.Code, 300, hooakhPicture);
            }
        }

        public static void SetMetadata(SaveSmokeMetadataModel model, SmokeSessionMetaData metadata)
        {
            if (!string.IsNullOrEmpty(model.Hookah))
                metadata.PipeId = int.Parse(model.Hookah);

            if (!string.IsNullOrEmpty(model.Bowl))
                metadata.BowlId = int.Parse(model.Bowl);


            metadata.HeatKeeper = (HeatKeeper) model.HeatKeeper;

            metadata.PackType = (PackType) model.PackType;

            metadata.CoalType = (CoalType) model.CoalType;


            metadata.CoalsCount = model.CoalsCount;
        }


        private async Task SentPercentageToDevice(Hookah hookah, Tobacco metaDataTobacco)
        {
            if (hookah.Version < 1000011)
                return;

            var percentage = 300;

            if (metaDataTobacco.Statistics != null)
                percentage = (int) metaDataTobacco.Statistics.PufCount;

            if (!await IotDeviceHelper.GetState(hookah.Code))
                return;

            await IotDeviceHelper.SendMsgToDevice(hookah.Code, $"stat:{percentage}:");
        }

        [HttpPost]
        public async Task<JsonResult> AssignPerson(string id, bool assign)
        {
            var smokeSession = _db.SmokeSessions.Include(session => session.Persons)
                .FirstOrDefault(a => a.SessionId == id);

            if (smokeSession == null)
                return Json(new {success = false});

            var result = await AssignToSmokeSession(smokeSession, assign, true);

            if (result)
                return Json(new {success = true});

            return Json(new {success = false});
        }


        public static Dictionary<string, DynamicSmokeStatistic> GetDynamicSmokeStatistic(List<Hookah> hookah,
            Func<Hookah, string> getCode)
        {
            var result = new Dictionary<string, DynamicSmokeStatistic>();
            using (var redis = RedisHelper.redisManager.GetClient())
            {
                foreach (var hookah1 in hookah)
                {
                    var session = RedisHelper.GetSmokeSessionId(hookah1.Code, redis);
                    var ds = redis.As<DynamicSmokeStatistic>()["DS:" + session];

                    if (ds != null)
                        result.Add(getCode(hookah1), ds);
                }
            }
            return result;
        }
    }
}