using smartHookah.Models.Db;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    [Authorize]
    public class BowlsController : Controller
    {
        private readonly SmartHookahContext db;

        public BowlsController(SmartHookahContext db)
        {
            this.db = db;
        }

        [Authorize]
        // GET: Bowls
        public async Task<ActionResult> Index()
        {
            return View(await db.Bowls.ToListAsync());
        }

        // GET: Bowls/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bowl bowl = await db.Bowls.FindAsync(id);
            if (bowl == null)
            {
                return HttpNotFound();
            }
            return View(bowl);
        }

        // GET: Bowls/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.BrandId = new SelectList(db.Brands.Where(b => b.Bowl == true), "Name", "Name");
            return View();
        }

        // POST: Bowls/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,AccName,BrandName")] Bowl bowl, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                bowl.Brand = await db.Brands.FindAsync(bowl.BrandName);
                db.Bowls.Add(bowl);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.BrandId = new SelectList(db.Brands.Where(b => b.Bowl == true), "Name", "Name");
            return View(bowl);
        }

        // GET: Bowls/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bowl bowl = await db.Bowls.FindAsync(id);
            if (bowl == null)
            {
                return HttpNotFound();
            }
            return View(bowl);
        }

        // POST: Bowls/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,AccName,BrandName")] Bowl bowl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bowl).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bowl);
        }

        // GET: Bowls/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bowl bowl = await db.Bowls.FindAsync(id);
            if (bowl == null)
            {
                return HttpNotFound();
            }
            return View(bowl);
        }

        // POST: Bowls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Bowl bowl = await db.Bowls.FindAsync(id);
            db.Bowls.Remove(bowl);
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
    }


}
