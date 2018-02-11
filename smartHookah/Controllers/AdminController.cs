using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    [Authorize(Roles = "Admin")]
    
    public class AdminController : Controller
    {
        private SmartHookahContext db;

        public AdminController(SmartHookahContext context)
        {
            this.db = context;
        }
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CleanEmptySmokeSession()
        {
            var smokeSession = db.SmokeSessions.Where(a => a.Statistics == null).Include(a => a.Hookah).ToList();

            var smokeSessionToDelete = smokeSession.Where(a => a.Hookah.SessionCode != a.SessionId);

            var sessionToDelete = smokeSessionToDelete as SmokeSession[] ?? smokeSessionToDelete.ToArray();
            db.SmokeSessions.RemoveRange(sessionToDelete);
            db.SessionMetaDatas.RemoveRange(sessionToDelete.Where(a => a.MetaData != null).Select(a => a.MetaData));

            db.SaveChanges();
            return View(sessionToDelete.Count());
        }

        public ActionResult CleanShortSession()
        {
            var test = db.DbPufs.Where(a => a.SmokeSession == null);

            var smokesession = db.SmokeSessions.Where(a => a.Statistics != null && a.Statistics.PufCount < 20);
            db.SmokeSessions.RemoveRange(smokesession);
            db.SessionMetaDatas.RemoveRange(smokesession.Where(a => a.MetaData != null).Select(a => a.MetaData));
            db.DbPufs.RemoveRange(smokesession.SelectMany(a => a.Pufs));
            db.SaveChanges();
            return View("CleanEmptySmokeSession", smokesession.Count());
        }

    }
}