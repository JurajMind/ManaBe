using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class GameController : Controller
    {
        private readonly SmartHookahContext db;

        public GameController(SmartHookahContext db)
        {
            this.db = db;
        }

        public ActionResult Snow(string id)
        {
            var model = new SnowViewModel();
            model.sessionId = id;



            return View(model);
        }


        public ActionResult Halloween(string id)
        {
            var model = new SnowViewModel();
            model.sessionId = id;



            return View(model);
        }

        public ActionResult ChcGame()
        {
            var person = UserHelper.GetCurentPerson(db);

            if (person == null)
                return RedirectToAction("LogIn", "Account");

            return View(person);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChcGamePost(int value)
        {
            var person = UserHelper.GetCurentPerson(db);
            person.Game = new GamePerson() {Value = value};

            db.Persons.AddOrUpdate(person);

            db.SaveChanges();

            return View(value);

        }

        [Authorize(Roles = "Admin")]
        public ActionResult GameResult()
        {
            var model = new GameResultModel();
            model.Persons = db.Persons.Where(a => a.Game != null).ToList();
            model.Users = db.Users.ToList();

            var mint = 558;

            var allTaste = 2584;

            var lemon = 344;

            model.Mint = mint;
            model.All = allTaste;
            model.lemon = lemon;

            return View(model);
        }
    }

    public class GameResultModel
    {
        public int lemon;
        public List<Person> Persons { get; set; }
        public int Mint { get; set; }
        public int All { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class SnowViewModel
    {
        public string sessionId;
    }
    

}