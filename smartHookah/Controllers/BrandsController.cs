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
        public async Task<ActionResult> Create([Bind(Include = "DisplayName,Url,Picture,Tobacco,Bowl,Hookah,TobaccoMixBrand")] Brand brand, HttpPostedFileBase file, FormCollection collection)
        {
            if (ModelState.IsValid)
            {

                var path = $"/Content/BrandPicture/";

                var displayArray = brand.Name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
                brand.Name = new string(displayArray);
        

                if (file != null)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName);
                    brand.Picture = path + brand.DisplayName + extension;

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
        public async Task<ActionResult> Edit([Bind(Include = "DisplayName,Url,Picture,Tobacco,Bowl,Hookah,TobaccoMixBrand")] Brand brand)
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
