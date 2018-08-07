using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Accord;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookahCommon;
using smartHookah.Support;

namespace smartHookah.Controllers
{
    [Authorize]
    public class HookahsController : Controller
    {
        private readonly SmartHookahContext db;
        private readonly IRedisService _redisService;

        public HookahsController(SmartHookahContext db, IRedisService redisService)
        {
            this.db = db;
            _redisService = redisService;
        }

        // GET: Hookahs
        public ActionResult All()
        {
            return View("Index",db.Hookahs.ToList());
        }

        public ActionResult Index()
        {
            var person = Helpers.UserHelper.GetCurentPerson();

            return View("Index", person.Hookahs.ToList());
        }

        public async Task<ActionResult> Restart()
        {
           await IotDeviceHelper.SendMsgToDevice("beta4", "restart:");
            return null;
        }

        [HttpPost]
        public async Task<ActionResult> ChangeName(int id,string newName)
        {
            var hookah = await db.Hookahs.FindAsync(id);
            if (hookah == null)
                return RedirectToAction("Index", "Home");

            var person = UserHelper.GetCurentPerson(db);

            if (hookah.Owners.Count(a => a.Id == person.Id) < 1) 
                return RedirectToAction("Index", "Home");

            hookah.Name = newName;

            db.Hookahs.AddOrUpdate(hookah);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new {id = hookah.Id});
        }

        [HttpPost]
        public async Task<ActionResult> ChangeOffline(int id, bool status)
        {
            var hookah = await db.Hookahs.FindAsync(id);
            if (hookah == null)
                return RedirectToAction("Index", "Home");

            var person = UserHelper.GetCurentPerson(db);

            if (hookah.Owners.Count(a => a.Id == person.Id) < 1)
                return RedirectToAction("Index", "Home");

            hookah.Offline = status;

            db.Hookahs.AddOrUpdate(hookah);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = hookah.Id });
        }

        [HttpPost]
        public async Task<ActionResult> ChangeSleep(int id, bool status)
        {
            var hookah = await db.Hookahs.FindAsync(id);
            if (hookah == null)
                return RedirectToAction("Index", "Home");

            var person = UserHelper.GetCurentPerson(db);

            if (hookah.Owners.Count(a => a.Id == person.Id) < 1)
                return RedirectToAction("Index", "Home");

            hookah.AutoSleep = status;

            db.Hookahs.AddOrUpdate(hookah);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = hookah.Id });
        }

        // GET: Hookahs/Details/5
        public ActionResult Details(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hookah hookah = db.Hookahs.Find(id);
            if (hookah == null)
            {
                return HttpNotFound();
            }
            var model = new HookahDetailsViewModel();
            model.Hookah = hookah;

            model.Updates = db.Updates.ToList()
                .ToSelectedList(a => a.Id.ToString(), a => $"{a.ReleseDate:dd.MM.yyyy}\t{ @Helper.UpdateVersionToString(a.Version)}\t:RN:{a.ReleseNote}");
            model.DeviceSetting = DeviceControlController.GetDeviceSettingViewModel(hookah.Setting,hookah.Version);
            model.DeviceSetting.SessionId = _redisService.GetSmokeSessionId(hookah.Code);
            model.Pictures = new SelectList(db.StandPictures, "id", "id", model.Hookah.Setting.Picture);
            return View(model);
        }

        // GET: Hookahs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Hookahs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Hookah hookah)
        {
            if (ModelState.IsValid)
            {
                db.Hookahs.Add(hookah);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hookah);
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateBatch()
        {
            return View();
        }

        [HttpPost]
        [ActionName("CreateBatch")]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateBatchPost(CreateBatchModel model)
        {
            var person = db.Persons.Find(model.PersonId);

            var modelStand = db.Hookahs.FirstOrDefault(a => a.Code == model.ModelStandName);

            if (person == null || modelStand == null || model.StartIndex > model.EndIndex)
                return RedirectToAction("CreateBatch");

            for (int i = model.StartIndex; i < model.EndIndex +1; i++)
            {
                var hookah = new Hookah(modelStand, _redisService);
                hookah.Code = string.Format(model.CodePattern, i);
                hookah.Name = string.Format(model.NamePattern, i);
                hookah.Owners = new List<Person>();
                hookah.Owners.Add(person);

                db.Hookahs.Add(hookah);
            }

            db.SaveChanges();

            return RedirectToAction("ShowGear","Person",new {id = person.Id});
        }

        // GET: Hookahs/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hookah hookah = db.Hookahs.Find(id);
            if (hookah == null)
            {
                return HttpNotFound();
            }
            return View(hookah);
        }

        // POST: Hookahs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Name")] Hookah hookah)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hookah).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(hookah);
        }

        // GET: Hookahs/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hookah hookah = db.Hookahs.Find(id);
            if (hookah == null)
            {
                return HttpNotFound();
            }
            return View(hookah);
        }

        // POST: Hookahs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(string id)
        {
            Hookah hookah = db.Hookahs.Find(id);
            db.Hookahs.Remove(hookah);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Assign()
        {
            var model = new AssignHookahViewModel();
            return View();
        }



        [HttpPost,ActionName("Assign")]
        [ValidateAntiForgeryToken]
        public ActionResult AssignHookahConfirmed(AssignHookahViewModel model)
        {

            if(ModelState.IsValid)
            { 
            Hookah hookah = db.Hookahs.FirstOrDefault(a => a.Code==model.HookahCode);
            Person person = new Person();

            if (!string.IsNullOrEmpty(model.personId))
            {
                person = db.Persons.Find(int.Parse(model.personId));
            }
            else
            {
                 person = Helpers.UserHelper.GetCurentPerson(db);
            }
       

            if(hookah == null || person == null)
                return RedirectToAction("Index");

         
        
            hookah.Owners.Add(person);
                db.Hookahs.AddOrUpdate(hookah);
            db.SaveChanges();
            }
            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("UnAssignHookah")]
        public async Task<ActionResult> UnAssignHookahPost(int? id)
        {
            var hookah = await db.Hookahs.FindAsync(id);

            var person = UserHelper.GetCurentPerson(db);


            if (hookah == null || person == null)
            {
                return RedirectToAction("MyGear", "Person");
            }

            hookah.Owners.Remove(person);

            db.Hookahs.AddOrUpdate(hookah);

            await db.SaveChangesAsync();

            return RedirectToAction("MyGear", "Person");
        }

        [HttpGet]
        public async Task<ActionResult> UnAssignHookah(int? id)
        {
            var hookah = await db.Hookahs.FindAsync(id);

            var person = UserHelper.GetCurentPerson(db);


            if (hookah == null || person == null)
            {
                return RedirectToAction("MyGear", "Person");
            }

            return View(hookah);
        }

        public class AssignHookahViewModel
        {
            public string HookahCode { get; set; }

            public string personId { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }

    public class CreateBatchModel
    {
        public int PersonId { get; set; }

        public string CodePattern { get; set; }

        public string NamePattern { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public string ModelStandName { get; set; }


    }

    public class HookahDetailsViewModel
    {
        public Hookah Hookah { get; set; }
        public DeviceControlController.DeviceSettingViewModel DeviceSetting { get; set; }
        public SelectList Pictures { get; set; }
        public SelectList Updates { get; set; }
    }
}
