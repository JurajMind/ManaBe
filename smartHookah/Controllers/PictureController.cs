using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Device;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    [Authorize]
    public class PictureController : Controller
    {
        private SmartHookahContext db = new SmartHookahContext();

        // GET: Picture
        public async Task<ActionResult> Index()
        {
            return View(await db.StandPictures.ToListAsync());
        }

        // GET: Picture/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StandPicture standPicture = await db.StandPictures.FindAsync(id);
            if (standPicture == null)
            {
                return HttpNotFound();
            }
            return View(standPicture);
        }

        // GET: Picture/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Picture/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(HttpPostedFileBase file)
        {
            var standPicture = new StandPicture();

            var filename = file.FileName;

            System.Drawing.Image sourceimage =
                System.Drawing.Image.FromStream(file.InputStream);
            var bitmap = (Bitmap)sourceimage;

            var trimmed = PictureHelper.TrimBitmap(bitmap);

            var xbm = PictureHelper.BmpToXbm(trimmed);

            var pictureString = PictureHelper.XbmToWebString(xbm);

            standPicture.PictueString = pictureString;
            standPicture.Width = trimmed.Width;
            standPicture.Height = trimmed.Height;

            db.StandPictures.Add(standPicture);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");


            return View(standPicture);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPicture(int id, int picture)
        {
            var hookah = await db.Hookahs.FindAsync(id);
            StandPicture standPicture = await db.StandPictures.FindAsync(picture);
            if (standPicture == null || hookah == null)
            {
                return HttpNotFound();
            }
            hookah.Setting.Picture = standPicture;

            db.HookahSettings.AddOrUpdate(hookah.Setting);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", "Hookahs", new { id = id });

        }

        // GET: Picture/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StandPicture standPicture = await db.StandPictures.FindAsync(id);
            if (standPicture == null)
            {
                return HttpNotFound();
            }
            return View(standPicture);
        }

        // POST: Picture/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,PictueString,Width,Height")] StandPicture standPicture)
        {
            if (ModelState.IsValid)
            {
                db.Entry(standPicture).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(standPicture);
        }

        [Authorize(Roles = "Admin")]
        // GET: Picture/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StandPicture standPicture = await db.StandPictures.FindAsync(id);
            if (standPicture == null)
            {
                return HttpNotFound();
            }
            return View(standPicture);
        }

        // POST: Picture/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            StandPicture standPicture = await db.StandPictures.FindAsync(id);
            db.StandPictures.Remove(standPicture);
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
