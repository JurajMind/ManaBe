using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using smartHookah.Services.Device;

namespace smartHookah.Controllers
{
    using MailChimp.Net;
    using MailChimp.Net.Interfaces;
    using MailChimp.Net.Models;
    using System.Configuration;
    using System.Threading.Tasks;

    [Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private SmartHookahContext db;
        private readonly ISmokeSessionService smokeSessionService;
        private readonly ISmokeSessionBgService smokeSessionBg;
        private readonly IRedisService redisService;
        private readonly IDeviceService deviceService;

        private IMailChimpManager manager;
        public AdminController(SmartHookahContext context, ISmokeSessionService smokeSessionService, IRedisService redisService, ISmokeSessionBgService smokeSessionBg, IDeviceService deviceService)
        {
            this.db = context;
            this.smokeSessionService = smokeSessionService;
            this.redisService = redisService;
            this.smokeSessionBg = smokeSessionBg;
            this.deviceService = deviceService;
            this.manager = new MailChimpManager(ConfigurationManager.AppSettings["MailChimpApiKey"]);
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CleanEmptySmokeSession()
        {
            var smokeSession = db.SmokeSessions.Where(a => a.Statistics == null).Include(a => a.Hookah).ToList();

            var smokeSessionToDelete = smokeSession.Where(a => redisService.GetSessionId(a.Hookah.Code) != a.SessionId && a.SessionReview == null);
            foreach (var session in smokeSessionToDelete)
            {
                try
                {
                    db.SmokeSessions.Remove(session);
                    if (session.MetaData != null)
                    {
                        db.SessionMetaDatas.Remove(session.MetaData);
                    }

                    db.SaveChanges();
                    this.redisService.RemoveSession(session.SessionId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                }


            }



            return View(smokeSessionToDelete.Count());
        }

        public ActionResult CleanShortSession()
        {
            var test = db.DbPufs.Where(a => a.SmokeSession == null);

            var smokesession = db.SmokeSessions.Where(a => a.Statistics != null && a.Statistics.PufCount < 20);
            db.SmokeSessions.RemoveRange(smokesession);
            db.SessionMetaDatas.RemoveRange(smokesession.Where(a => a.MetaData != null).Select(a => a.MetaData));
            db.DbPufs.RemoveRange(smokesession.SelectMany(a => a.DbPufs));
            db.SaveChanges();
            return View("CleanEmptySmokeSession", smokesession.Count());
        }

        public ActionResult StoreOldSessions()
        {
            smokeSessionService.StoreOldPufs();
            return null;
        }

        public ActionResult InitNewSessions()
        {
            smokeSessionBg.IntNewSessions();
            return null;
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> FixSessions()
        {
            var devices = this.db.Hookahs.Select(s => s.Code).ToList();


            foreach (var device in devices)
            {
                var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == device);

                if (hookah == null)
                    continue;

                var sessions = this.db.SmokeSessions.Where(s => s.HookahId == hookah.Id);

                if (sessions.Count(s => s.StatisticsId == null) == 0)
                {
                    await this.smokeSessionBg.InitSmokeSession(device);
                    continue;
                }

                if (sessions.Count() != 0)
                    continue;


                await this.smokeSessionBg.InitSmokeSession(device);
            }

            return null;
        }

        public ActionResult StoreBrands()
        {
            var brands = this.db.Brands.Select(a => a.DisplayName).ToList();
            this.redisService.StoreBrands(brands);
            return null;
        }

        public async Task<ActionResult> UpdateMailChimp()
        {
            var listId = "5ec4bd2f5a";

            var users = db.Users;

            foreach (var applicationUser in users)
            {
                var member = new Member { EmailAddress = applicationUser.Email, StatusIfNew = Status.Subscribed };
                // Use the Status property if updating an existing member

                member.MergeFields.Add("NAME", applicationUser.Person.DisplayName);
                member.MergeFields.Add("HASH", applicationUser.GetHash());
                await this.manager.Members.AddOrUpdateAsync(listId, member);
            }

            return null;

        }

        public async Task<ActionResult> DeleteDevice(string code)
        {
            await this.deviceService.DeleteDevice(code);

            return null;

        }

        public async Task<ActionResult> CleanDevice()
        {

            var devices = this.db.Hookahs.Where(a => a.Code.StartsWith("tear20200117")).ToList();
            foreach (var device in devices)
            {
                await this.DeleteDevice(device.Code);
            }

            return null;
        }

    }
}