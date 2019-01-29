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
using smartHookah.Migrations;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    [Authorize]
    public class BrandsController : Controller
    {
        private readonly SmartHookahContext db;

        public BrandsController(SmartHookahContext db)
        {
            this.db = db;
        }

        // GET: Brands
        public async Task<ActionResult> Index()
        {
            return View(await db.Brands.ToListAsync());
        }

        // GET: Brands/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Brand brand = await db.Brands.FindAsync(id);
            if (brand == null)
            {
                return HttpNotFound();
            }
            return View(brand);
        }

        // GET: Brands/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DisplayName,Url,Picture,TobaccoSimple,Bowl,Hookah,Coal,HeatManagment,TobaccoMixBrand")] Brand brand, HttpPostedFileBase file, FormCollection collection)
        {
            if (ModelState.IsValid)
            {

                var path = $"/Content/BrandPicture/";

                var displayArray = brand.DisplayName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
                brand.Name = new string(displayArray);
        

                if (file != null)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName);
                    brand.Picture = path + brand.Name + extension;

                    file.SaveAs(Server.MapPath(brand.Picture));
                }

                db.Brands.Add(brand);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(brand);
        }

        public ActionResult FixName()
        {

            var brands = db.Brands.ToList();
            foreach (var brand in brands)
            {
                brand.DisplayName = brand.Name;
                db.Brands.AddOrUpdate(brand);
            }

            db.SaveChanges();

            return null;


        }

        // GET: Brands/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Brand brand = await db.Brands.FindAsync(id);
            if (brand == null)
            {
                return HttpNotFound();
            }
            return View(brand);
        }

        // POST: Brands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,DisplayName,Url,Picture,TobaccoSimple,Bowl,Hookah,Coal,HeatManagment,TobaccoMixBrand")] Brand brand)
        {
            if (ModelState.IsValid)
            {
                db.Entry(brand).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(brand);
        }

        // GET: Brands/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Brand brand = await db.Brands.FindAsync(id);
            if (brand == null)
            {
                return HttpNotFound();
            }
            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Brand brand = await db.Brands.FindAsync(id);
            db.Brands.Remove(brand);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

         [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GeneratePictures(string id)
        {
            var brands = this.db.Brands;
            var random = new Random();
            var colors = new List<string> { "1F03FF", "19F7AD", "000000", "F41CF4" };
            var path = $"/Content/BrandPicture/";
            var format = "https://dummyimage.com/300x300/000/fff.jpg&text={1}";
            var result = new List<string>();
            foreach (var brand in brands)
            {
                var colorIndex = random.Next(0, colors.Count);
                
                var name = string.Join(" ",brand.DisplayName.Split(new []{' '},StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToList());
                if (string.IsNullOrEmpty(name))
                {
                    name = 0.ToString();
                }

                var url = String.Format(format, colors[colorIndex], name);

                var uri = new Uri(url);
                var filename = brand.Name + ".jpg";
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(uri, Server.MapPath(path + filename));
                    brand.Picture = path + filename;
                    db.Brands.AddOrUpdate(brand);

                }
               

                result.Add(brand.Picture);
            }

            db.SaveChanges();

            ViewBag.Result = result;
            return View();
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
