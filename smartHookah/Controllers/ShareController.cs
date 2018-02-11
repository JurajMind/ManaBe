using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class ShareController : Controller
    {

        private readonly SmartHookahContext db;

        public ShareController(SmartHookahContext db)
        {
            this.db = db;
        }
        [AllowAnonymous]
        public async Task<ActionResult> Index(string id)
        {
            var session = db.SmokeSessions.FirstOrDefault(a => a.Token == id);

            if (session != null)
            {

                if (session.Pufs.Any())
                {
                    var statsModel = new SmokeSessionGetStaticsticsModel();
                    statsModel.Create(db, session.Id);
                    statsModel.Share = true;
                    return View("~/Views/SmokeSession/GetStatistics.cshtml", statsModel);
                }

                var model = new SmokeViewModel();
               await model.CreateModel(db, session.Id);
                model.Share = true;
                return View("Smoke", model);;
            }
            
            return null;
        }
        [Authorize]
        public ActionResult UpdateToken()
        {

            foreach (SmokeSession dbSmokeSession in db.SmokeSessions)
            {
                dbSmokeSession.Token = Support.Support.RandomString(10);
               db.SmokeSessions.AddOrUpdate(dbSmokeSession);
            }
            db.SaveChanges();
            return null;
        }
         [Authorize]

        public ActionResult ShareStand(string id)
        {
            return null;
        }

        public static string GetToken(int key)
        {
            var token =  Convert.ToBase64String(MachineKey.Protect(BitConverter.GetBytes(key)));

            var a = DecoteToken(token);
            return token;
        }

        public static int DecoteToken(string token)
        {
          var bToken = Convert.FromBase64String(token);
           var bInt =  MachineKey.Unprotect(bToken);
           return BitConverter.ToInt32(bInt,0);

        }
    }
}