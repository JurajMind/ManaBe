using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    [Authorize]
    public class PipesController : Controller
    {
        private readonly SmartHookahContext db;

        public PipesController(SmartHookahContext db)
        {
            this.db = db;
        }

        // GET: Pipes
        public async Task<ActionResult> Index()
        {
            return View(await db.Pipes.ToListAsync());
        }

        // GET: Pipes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pipe pipe = await db.Pipes.FindAsync(id);
            if (pipe == null)
            {
                return HttpNotFound();
            }
            return View(pipe);
        }

        // GET: Pipes/Create
        public ActionResult Create()
        {
            ViewBag.BrandId = new SelectList(db.Brands.Where(b => b.Hookah == true), "Name", "Name");
            return View();
        }

        // POST: Pipes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,AccName,BrandName")] Pipe pipe)
        {
            if (ModelState.IsValid)
            {
                pipe.Brand = await db.Brands.FindAsync(pipe.BrandName);
                db.Pipes.Add(pipe);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(pipe);
        }
        [Authorize(Roles = "Admin")]
        // GET: Pipes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pipe pipe = await db.Pipes.FindAsync(id);
            if (pipe == null)
            {
                return HttpNotFound();
            }
            return View(pipe);
        }

        // POST: Pipes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,AccName,BrandName")] Pipe pipe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pipe).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(pipe);
        }

        // GET: Pipes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pipe pipe = await db.Pipes.FindAsync(id);
            if (pipe == null)
            {
                return HttpNotFound();
            }
            return View(pipe);
        }

        // POST: Pipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Pipe pipe = await db.Pipes.FindAsync(id);
            db.Pipes.Remove(pipe);
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
