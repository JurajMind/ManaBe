using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;

namespace smartHookah.Controllers
{
    using System.Configuration;
    using System.Threading.Tasks;

    using MailChimp.Net;
    using MailChimp.Net.Interfaces;
    using MailChimp.Net.Models;

    [Authorize(Roles = "Admin")]
    
    public class AdminController : Controller
    {
        private SmartHookahContext db;
        private readonly ISmokeSessionService smokeSessionService;
        private readonly ISmokeSessionBgService smokeSessionBg;
        private readonly IRedisService redisService;

        private IMailChimpManager manager;
        public AdminController(SmartHookahContext context, ISmokeSessionService smokeSessionService, IRedisService redisService, ISmokeSessionBgService smokeSessionBg)
        {
            this.db = context;
            this.smokeSessionService = smokeSessionService;
            this.redisService = redisService;
            this.smokeSessionBg = smokeSessionBg;
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

            var smokeSessionToDelete = smokeSession.Where(a => redisService.GetSessionId(a.Hookah.Code) != a.SessionId && a.Review == null);

            var sessionToDelete = smokeSessionToDelete as SmokeSession[] ?? smokeSessionToDelete.ToArray();
            db.SmokeSessions.RemoveRange(sessionToDelete);
            db.SessionMetaDatas.RemoveRange(sessionToDelete.Where(a => a.MetaData != null).Select(a => a.MetaData));
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {

            }

            return View(sessionToDelete.Count());
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

    }
}