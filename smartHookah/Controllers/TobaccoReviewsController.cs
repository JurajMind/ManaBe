using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class TobaccoReviewsController : Controller
    {
        private SmartHookahContext db = new SmartHookahContext();

        // GET: TobaccoReviews
        public async Task<ActionResult> Index()
        {
            var tobaccoReviews = db.TobaccoReviews.Include(t => t.Author).Include(t => t.ReviewedTobacco).Include(t => t.SmokeSession);
            return View(await tobaccoReviews.ToListAsync());
        }

        // GET: TobaccoReviews/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TobaccoReview tobaccoReview = await db.TobaccoReviews.FindAsync(id);
            if (tobaccoReview == null)
            {
                return HttpNotFound();
            }
            return View(tobaccoReview);
        }

        // GET: TobaccoReviews/Create
        public ActionResult Create()
        {
            ViewBag.AuthorId = new SelectList(db.Persons, "Id", "Id");
            ViewBag.ReviewedTobaccoId = new SelectList(db.Tobaccos, "Id", "AccName");
            ViewBag.SmokeSessionId = new SelectList(db.SmokeSessions, "Id", "SessionId");
            return View();
        }

        // POST: TobaccoReviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,AuthorId,PublishDate,Quality,Taste,Smoke,Overall,Text,ReviewedTobaccoId,SmokeSessionId")] TobaccoReview tobaccoReview)
        {
            if (ModelState.IsValid)
            {
                db.TobaccoReviews.Add(tobaccoReview);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorId = new SelectList(db.Persons, "Id", "Id", tobaccoReview.AuthorId);
            ViewBag.ReviewedTobaccoId = new SelectList(db.Tobaccos, "Id", "AccName", tobaccoReview.ReviewedTobaccoId);
            ViewBag.SmokeSessionId = new SelectList(db.SmokeSessions, "Id", "SessionId", tobaccoReview.SmokeSessionId);
            return View(tobaccoReview);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateFromModal([Bind(Include = "Id,Quality,Taste,Smoke,Overall,Text,SmokeSessionId")] TobaccoReview tobaccoReview)
        {
            if (ModelState.IsValid)
            {
                var smokeSession = await db.SmokeSessions.FindAsync(tobaccoReview.SmokeSessionId);
                tobaccoReview.PublishDate = DateTime.Now;
                if (smokeSession.MetaData.TobaccoId.HasValue)
                    tobaccoReview.ReviewedTobaccoId = smokeSession.MetaData.TobaccoId.Value;
                else
                {
                    return Json(new {success = false , msg = "Please fill tobacco information first"});
                }

                if (UserHelper.GetCurentPerson() == null)
                {
                    return Json(new { success = false, msg = "Please log in first" });
                }
                
                tobaccoReview.AuthorId = UserHelper.GetCurentPerson().Id;
                smokeSession.Review = tobaccoReview;
                db.SmokeSessions.AddOrUpdate(smokeSession);
                db.TobaccoReviews.AddOrUpdate(tobaccoReview);
                await db.SaveChangesAsync();
                return Json(new { success = true, page = RazorViewToString.RenderRazorViewToString(this, "~/Views/TobaccoReviews/smokeSessionReview.cshtml", tobaccoReview) });

            }

            return Json(new { success = false });

        }

        public static async Task<TobaccoReview> GetSessionReview(int sessionId, SmartHookahContext db)
        {
            var session = db.SmokeSessions.FindAsync(sessionId);

            if (session == null)
                new TobaccoReview();

            var review = db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == sessionId);

            if(review == null)
                return new TobaccoReview();
            return review;
        }

        // GET: TobaccoReviews/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TobaccoReview tobaccoReview = await db.TobaccoReviews.FindAsync(id);
            if (tobaccoReview == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = new SelectList(db.Persons, "Id", "Id", tobaccoReview.AuthorId);
            ViewBag.ReviewedTobaccoId = new SelectList(db.Tobaccos, "Id", "AccName", tobaccoReview.ReviewedTobaccoId);
            ViewBag.SmokeSessionId = new SelectList(db.SmokeSessions, "Id", "SessionId", tobaccoReview.SmokeSessionId);
            return View(tobaccoReview);
        }

        // POST: TobaccoReviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,AuthorId,PublishDate,Quality,Taste,Smoke,Overall,Text,ReviewedTobaccoId,SmokeSessionId")] TobaccoReview tobaccoReview)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tobaccoReview).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorId = new SelectList(db.Persons, "Id", "Id", tobaccoReview.AuthorId);
            ViewBag.ReviewedTobaccoId = new SelectList(db.Tobaccos, "Id", "AccName", tobaccoReview.ReviewedTobaccoId);
            ViewBag.SmokeSessionId = new SelectList(db.SmokeSessions, "Id", "SessionId", tobaccoReview.SmokeSessionId);
            return View(tobaccoReview);
        }

        // GET: TobaccoReviews/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TobaccoReview tobaccoReview = await db.TobaccoReviews.FindAsync(id);
            if (tobaccoReview == null)
            {
                return HttpNotFound();
            }
            return View(tobaccoReview);
        }

        // POST: TobaccoReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TobaccoReview tobaccoReview = await db.TobaccoReviews.FindAsync(id);
            db.TobaccoReviews.Remove(tobaccoReview);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public async Task<JsonResult> GetReviewVue(int? sessionId)
        {
            var review = (sessionId == null) ? null : db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == sessionId);
            
            return (review != null) ? Json(review, JsonRequestBehavior.AllowGet) : null;
        }

        [HttpPost]
        public async Task<JsonResult> SaveVueReview([Bind(Include = "Id,AuthorId,PublishDate,Quality,Taste,Smoke,Overall,Text,ReviewedTobaccoId,SmokeSessionId")] TobaccoReview tobaccoReview)
        {
            if (ModelState.IsValid)
            {
                db.TobaccoReviews.AddOrUpdate(tobaccoReview);
                await db.SaveChangesAsync();
                return Json(tobaccoReview, JsonRequestBehavior.AllowGet);
            }

            return null;
        }
    }
}
