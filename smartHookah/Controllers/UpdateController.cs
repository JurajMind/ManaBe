using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    public class UpdateController : Controller
    {
        private SmartHookahContext db = new SmartHookahContext();

        private readonly IRedisService _redisService;

        public UpdateController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        // GET: Update
        public async Task<ActionResult> Index()
        {
            return View(await db.Updates.ToListAsync());
        }

        // GET: Update/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Update update = await db.Updates.FindAsync(id);
            if (update == null)
            {
                return HttpNotFound();
            }
            return View(update);
        }

        // GET: Update/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Update/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "Id,Version,ReleseDate,ReleseNote,Type")] Update update, HttpPostedFileBase file,string versionString)
        {
            if (ModelState.IsValid && file != null)
            {
                var path = $"~/Updates/{versionString}/{update.Type}";
                var path32 = path + "/32/";
                var path60 = path + "/60/"; 
                update.Path = path32+file.FileName;
             

                update.Version = Helper.UpdateVersionToInt(versionString);

                if(!System.IO.Directory.Exists(Server.MapPath(path32)))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path32));
                if (!System.IO.Directory.Exists(Server.MapPath(path60)))
                    System.IO.Directory.CreateDirectory(Server.MapPath(path60));

                file.SaveAs(Server.MapPath(update.Path));

                update.ReleseDate = DateTime.Now;
                db.Updates.Add(update);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(update);
        }

        // GET: Update/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Update update = await db.Updates.FindAsync(id);
            if (update == null)
            {
                return HttpNotFound();
            }
            return View(update);
        }

        // POST: Update/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Version,ReleseDate,ReleseNote,Type,PathTo32,PathTo60")] Update update)
        {
            if (ModelState.IsValid)
            {
                db.Entry(update).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(update);
        }

        // GET: Update/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Update update = await db.Updates.FindAsync(id);
            if (update == null)
            {
                return HttpNotFound();
            }
            return View(update);
        }

        // POST: Update/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Update update = await db.Updates.FindAsync(id);
            db.Updates.Remove(update);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [AllowAnonymous]
        [OptionalHttps(true)]
        public FileResult Download(string id,string token)
        {
            var path = _redisService.GetUpdateFilePath(id, token);
            var filePath = Server.MapPath(path);
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            const string fileName = "update.bin";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        [Authorize]
        public async Task<JsonResult> PromptUpdate(int hookahId, int updateId )
        {
            var user = UserHelper.GetCurentPerson(db);
            
            try
            {

                if (!User.IsInRole("Admin"))
                {
                    var canUpdate = user.Hookahs.Any(a => a.Id == hookahId);
                    if (!canUpdate)
                        return Json(new { succes = false });
                }

                var hookah = await db.Hookahs.FindAsync(hookahId);
                var update = await db.Updates.FindAsync(updateId);
                var updateToken = Support.Support.RandomString(5);

                var updatePath = update.Path;
            
                var updateRedis = new UpdateRedis()
                {
                    FilePath = updatePath,
                    HookahCode = hookah.Code
                };

                _redisService.Update(updateRedis, updateToken);

                var msg = $"update:{updateToken}";

                await IotDeviceHelper.SendMsgToDevice(hookah.Code, msg);
            }
            catch (Exception e)
            {

                return Json(new {succes = false, msg = e.Message});
            }


            return Json(new {succes = true, msg = "Update was sent"});
        

    }

        public class UpdateRedis
        {
            public string FilePath { get; set; }

            public string HookahCode { get; set; }

            
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
}
