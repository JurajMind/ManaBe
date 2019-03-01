using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Accord.Statistics.Kernels;
using Microsoft.AspNet.Identity.Owin;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Models.Redis;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    using smartHookah.Services.Person;

    [Authorize]
    public class PersonController : Controller
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public PersonController(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

    
        // GET: Person
        public async Task<ActionResult> Index(int? id)
        {
            var persons = this.personService.GetCurentPersonIQuerable();

            var person = persons.Include(a => a.SmokeSessions.Select(b => b.MetaData))
                               .Include(a => a.SmokeSessions.Select(b => b.Statistics)).FirstOrDefault();

            var model = new PersonIndexViewModel();
            await model.Fill(person, db);
            //var sessions = person.SmokeSessions.Where(a => a.Statistics != null).OrderByDescending(a => a.Statistics.Start).Take(5);
           

            //var model = new PersonIndexViewModel();
            //model.SmokeSessions = sessions.ToList();
            //var hookahs = person.Hookahs.ToList();
            //model.OnlineHookah = await IotDeviceHelper.GetState(hookahs.Select(a => a.Code).ToList());
            
            //model.Hookah = hookahs;
            //model.ActiveSession = db.SmokeSessions.Where(a =>
            //        a.Persons.Any(p => p.Id == person.Id) && a.Statistics == null).ToList();


            //model.Person = person;
            //var activeHookah = model.ActiveSession.Select(a => a.Hookah).Union(hookahs).ToList();
            //ViewBag.Title = "Manapipes";
            //model.DynamicStatistic = SmokeSessionController.GetDynamicSmokeStatistic(activeHookah, a => a.SessionCode);
            ViewBag.Title = "Manapipes";
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> GetSmokeOverview(string from, string to,int?id)
        {
            var fromDate = DateTime.ParseExact(from, "dd.MM.yyyy",CultureInfo.InvariantCulture);
            var toDate = DateTime.ParseExact(to, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            var sessions = GetPersonSmokeSession(fromDate, toDate,id);
          
            return View("_PersonSmokeSessionDetails", sessions);
        }

        [Authorize]
        public ActionResult ManageColors(int? id, int? personId)
        {

            var person = UserHelper.GetCurentPerson(db, personId,true);
            if (person == null)
                return RedirectToAction("Index");

          

            var setting = person.DefaultSetting;

            if (setting == null)
            {
                setting = new DeviceSetting(person.Hookahs.FirstOrDefault().Setting);
            }

            var model = DeviceControlController.GetDeviceSettingViewModel(setting, 1000027, db);
            model.SessionId = $"Place{person.Id}";

            return View(model);
        }

        private List<SmokeSession> GetPersonSmokeSession(DateTime from, DateTime? to = null,int?id = null)
        {
            var toDate = to ?? DateTime.Now;
            var person = UserHelper.GetCurentPerson(db,id);
            return db.SmokeSessions.Where(
                    a =>
                        a.Persons.Any(p => p.Id == person.Id) && a.Statistics != null &&
                        a.Statistics.Start >= from
                        && a.Statistics.Start <= toDate).Include(x => x.MetaData.Pipe)
                .Include(x => x.MetaData.Tobacco)
                .Include(x => x.MetaData.Pipe.Brand)
                .Include(x => x.MetaData.Tobacco.Brand)
                .Include(x => x.Statistics).OrderByDescending(a => a.Id).ToList();
        }

        [Authorize(Roles =  "Admin")]
        public async Task<ActionResult> PersonList()
        {
            return View(await db.Persons.ToListAsync());
        }

        public async Task<ActionResult> SmokeSessions()
        {
            var user = UserHelper.GetCurentPerson(db);
            var user2 = db.Persons.Include(a => a.SmokeSessions).FirstOrDefault(u => u.Id == user.Id);
            var sessions = user2.SmokeSessions;
           

                     var model = new SmokeSessionViewModel();
            model.SmokeSessions = sessions.ToList();
            return View("~/Views/SmokeSession/Index.cshtml",model);
        }

        public async Task<ActionResult> DashBoard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = await db.Persons.FindAsync(id);

            ApplicationUser user =  db.Users.First(a => a.PersonId == id);

            if (person == null)
            {
                return HttpNotFound();
            }

            var session = db.SmokeSessions.Where(s => s.Hookah.Owners.Any(p => p.Id == id))
                .Include(a => a.Statistics)
                .Include(a => a.MetaData);
                
           

            var model = new DashBoardViewModel();

            var personHookahId = person.Hookahs.Select(h => h.Code).ToList();
            var onlienHookah = await IotDeviceHelper.GetState(personHookahId);

            var liveSeesionId = person.Hookahs.Where(h => onlienHookah.Contains(h.Code)).Select(h => RedisHelper.GetSmokeSessionId(h.Code)).ToList();

            var liveSessions = session.Where(s => liveSeesionId.Contains(s.SessionId));


            model.User = user;
            model.Sessions = session;
            model.SessionStatistics = session.Where(a => a.Statistics != null).Select(a => a.Statistics).ToList();
            model.MetaData = session.Where(a => a.MetaData != null).Select(a => a.MetaData).ToList();
            model.LiveSessions = liveSessions;

            return View(model);
        }

        // GET: Person/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = await db.Persons.FindAsync(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // GET: Person/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Person/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id")] Person person)
        {
            if (ModelState.IsValid)
            {
                db.Persons.Add(person);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = await db.Persons.FindAsync(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id")] Person person)
        {
            if (ModelState.IsValid)
            {
                db.Entry(person).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: Person/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = await db.Persons.FindAsync(id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Person person = await db.Persons.FindAsync(id);
            db.Persons.Remove(person);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> AddHookah(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Person person = await db.Persons.FindAsync(id);
            var hookahs = db.Hookahs;

            var model = new AddHookahViewModel();
            model.Person = person;
            model.Hookahs = await hookahs.ToListAsync();

            return View(model);
        }

        [HttpPost, ActionName("AddHookah")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddHookahPost(AddHookahPostModel model)
        {
            Person person = await db.Persons.FindAsync(model.PersonId);
            var hookah = await db.Hookahs.FindAsync(model.SelectedHookah);
            person.Hookahs.Add(hookah);
            await db.SaveChangesAsync();

            return RedirectToAction("Details");
        }


        public async Task<ActionResult> AssignSessions()
        {
            var hookahs = db.Hookahs.Include(hookah => hookah.Owners).Include(hookah => hookah.SmokeSessions);

            foreach (var hookah in hookahs)
            {
                foreach (var hookahOwner in hookah.Owners)
                {
                    foreach (SmokeSession session in hookah.SmokeSessions)
                    {
                        if (hookahOwner.SmokeSessions.Count(a => a.Id == session.Id) == 0)
                        {
                            hookahOwner.SmokeSessions.Add(session);
                        }
                    }
                    db.Persons.AddOrUpdate(hookahOwner);
               
                }
               
            }
            await db.SaveChangesAsync();
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public async Task<ActionResult> MyGear()
        {
            var person = UserHelper.GetCurentPerson();

            if (person == null)
                return null;

            return await ShowGear(person.Id);

        }
        
        public async Task<ActionResult> ShowGear(int id)
        {
            var curentPerson = UserHelper.GetCurentPerson();


            var person = db.Persons.Where(a => a.Id == id).Include(a => a.OwnedPipeAccesories).FirstOrDefault();

            //if (person.Place != null)
            //{
            //    if (person.Place.Managers.All(a => a.Id != curentPerson.Id))
            //    {
            //        return RedirectToAction("Index");
            //    }
            //}

            if (person == null)
                return null;

            var model = ShowGearViewModel(person);
         
            return View("ShowGear",model);
        }

        public async Task<ActionResult> MyStatistic(int? id)
        {
            var person = UserHelper.GetCurentPerson(db, id);
            var fromDate = DateTime.Now.AddDays(-7);
            var sessions = GetPersonSmokeSession(fromDate,id:id);
            var model = new MyStatisticViewModel();
            model.session = sessions;

            return View(model);
        }

     


        private ShowGearViewModel ShowGearViewModel(Person person)
        {
           
            var model = new ShowGearViewModel();
            model.Pipes = person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Pipe && a.DeleteDate == null).Select(a => a.PipeAccesory as Pipe).ToList();
            model.Bowls = person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Bowl && a.DeleteDate == null).Select(a => a.PipeAccesory as Bowl).ToList();
            model.Tobaccos = person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Tobacco && a.DeleteDate == null ).Select(a => a.PipeAccesory as Tobacco).ToList();
            model.HeatManagments = person.OwnedPipeAccesories.Where(a => a.PipeAccesory is HeatManagment && a.DeleteDate == null).Select(a => a.PipeAccesory as HeatManagment).ToList();
            model.Goals = person.OwnedPipeAccesories.Where(a => a.PipeAccesory is Coal && a.DeleteDate == null).Select(a => a.PipeAccesory as Coal).ToList();
            model.Person = person;
            var user = db.Users.First(a => a.PersonId == person.Id);

            model.DisplayName = user.DisplayName;
            var curentUser = UserHelper.GetCurentPerson(db);
            if (User.IsInRole("Admin") || (curentUser != null && UserHelper.GetCurentPerson(db).Id == person.Id))
                model.CanEdit = true;

            return model;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddGear(int id, int? amount, int? personId = null)
        {
            AddGear(id, amount, db, out var person,personId);

            var model = ShowGearViewModel(person);
            return View("_MyGear", model);
        }

        public static void AddGear(int id, int? amount, SmartHookahContext db,out Person person, int? personId = null)
        {
            
            if (!personId.HasValue)
                person = UserHelper.GetCurentPerson();
            else
            {
                person =  db.Persons.Find(personId.Value);
            }

            var newAccesory =  db.PipeAccesories.Find(id);

            if (person.OwnedPipeAccesories.Any(a => a.PipeAccesoryId == newAccesory.Id && !a.Deleted))
            {
                return;
            }

            person.OwnedPipeAccesories.Add(new OwnPipeAccesories()
            {
                Person = person,
                PipeAccesory = newAccesory,
                Amount = Convert.ToInt32(amount),
                CreatedDate =  DateTime.UtcNow,

            });

            db.Persons.AddOrUpdate(person);
             db.SaveChanges();

            return;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> RemoveGear(int id, int? personId = null)
        {
            Person person;
            if (!personId.HasValue)
                person = UserHelper.GetCurentPerson();
            else
            {
                person = await db.Persons.FindAsync(personId.Value);
            }

            var oldAccesory = person.OwnedPipeAccesories.FirstOrDefault(a => a.PipeAccesoryId == id);

            if (oldAccesory == null)
            {
                var model2 = ShowGearViewModel(person);
                return View("_MyGear", model2);
            }

            oldAccesory.DeleteDate = DateTime.UtcNow;
            
            db.OwnPipeAccesorieses.AddOrUpdate(oldAccesory);

            await db.SaveChangesAsync();


            var model = ShowGearViewModel(person);
            
            return View("_MyGear", model);

        }


    }


}
