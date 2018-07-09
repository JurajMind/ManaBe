using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    [Authorize]
    public class PipeAccesorryController : Controller
    {
        // GET: PipeAccesorry
        private readonly SmartHookahContext db;

        public PipeAccesorryController(SmartHookahContext db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            return View();
        }

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
        public async Task<ActionResult> Create([Bind(Include = "Id,AccName,BrandName")] PipeAccesory accesory, FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                var type = collection["type"];

                switch (type)
                {
                    case "bowl":
                    {
                        accesory = new Bowl(accesory);
                        break;
                    }
                    case "tobacco":
                        {
                            accesory = new Tobacco(accesory);
                            break;
                        }
                    case "pipe":
                    {
                            accesory = new Pipe(accesory); 
                            break;
                        }
                }
                var brand = await db.Brands.FindAsync(accesory.BrandName);


                var existing =
                    db.PipeAccesories.FirstOrDefault(a => a.AccName == accesory.AccName && a.BrandName == accesory.BrandName);

                

                if (brand == null)
                    return RedirectToAction("Create");

                accesory.Brand = brand;

                if (existing == null)
                {
                    db.PipeAccesories.Add(accesory);
                    db.SaveChanges();
                }
                else
                {
                    accesory = existing;
                }
                
                var add = bool.Parse(collection["add"]);

                if (add)
                {
                    var person = new Person();
                    PersonController.AddGear(accesory.Id,0,db,out person,UserHelper.GetCurentPerson(db).Id);
                    return RedirectToAction("MyGear", "Person");
                }

                switch (type)
                {
                    case "bowl":
                        {
                            return RedirectToAction("Index", "Bowls");
                        }
                    case "tobacco":
                        {
                            return RedirectToAction("Index", "Tobacco");
                        }
                    case "pipe":
                        {
                            return RedirectToAction("Index", "Pipes");
                        }
                }
            }
            ViewBag.BrandId = new SelectList(db.Brands.Where(b => b.Bowl == true), "Name", "Name");
            return View(accesory);
        }

        [HttpGet]
        public async Task<ActionResult> UploadPicture(int id)
        {
            var item = await db.PipeAccesories.FindAsync(id);

            return View(item);
        }

        public async Task<ActionResult> Details(int id)
        {
            var item = await db.PipeAccesories.FindAsync(id);

            if (item == null)
                return RedirectToAction("Index", "Home");

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadPicture(int id, HttpPostedFileBase file)
        {
            var item = await db.PipeAccesories.FindAsync(id);
            var path = $"/Content/ItemPicture/";
            if (file != null)
            {
                var extension = System.IO.Path.GetExtension(file.FileName);
                item.Picture = path +item.Brand.Name+"/"+ item.AccName + extension;

                System.IO.Directory.CreateDirectory(Server.MapPath(path + item.Brand.Name));


                file.SaveAs(Server.MapPath(item.Picture));
                db.PipeAccesories.AddOrUpdate(item);
                await db.SaveChangesAsync();
            }


            return RedirectToAction("Index", "Person");
        }

        public async Task<JsonResult> GetBrands(string type)
        {
            var brans = db.Brands;

            switch (type)
            {
                case "bowl":
                    return GetBrandFromQuery(brans.Where(a => a.Bowl));
                   
                case "pipe":
                    return GetBrandFromQuery(brans.Where(a => a.Hookah));
                case "tobacco":
                    return GetBrandFromQuery(brans.Where(a => a.Tobacco));
            }

            return Json(new {brans = new List<string>() {"None"} },JsonRequestBehavior.AllowGet);

        }

        public async Task<JsonResult> GetAllGear()
        {
            var pipes = db.Pipes.Select(a => new {id = a.Id, name = a.BrandName + " " + a.AccName});
            var bowls = db.Bowls.Select(a => new { id = a.Id, name = a.BrandName + " " + a.AccName });
            var tobacoBrands = db.Brands.Where(a => a.Tobacco).Select(a => a.Name).Distinct().Select(b => new { id = b, name = b });

            return Json(new {pipes, bowls, tobacoBrands}, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetMyGear()
        {
            
            var person = UserHelper.GetCurentPerson(db);
            if (person == null)
                return await GetAllGear();

            var pipes = person.Pipes.Select(a => new { id = a.Id, name = a.BrandName + " " + a.AccName });
            var bowls = person.Bowls.Select(a => new { id = a.Id, name = a.BrandName + " " + a.AccName });
            var tobacoBrands = person.Tobacco.Select(a => a.BrandName).Distinct().Select(b => new { id = b, name = b });

            return Json(new { pipes, bowls, tobacoBrands }, JsonRequestBehavior.AllowGet);
        }



        public async Task<JsonResult> GetName(string type, string brand)
        {
            var brans = await db.Brands.FindAsync(brand);

            switch (type)
            {
                case "bowl":
                    return Json(new { names = brans.PipeAccesories.Where(a => a is Bowl).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);

                case "pipe":
                    return Json(new { names = brans.PipeAccesories.Where(a => a is Pipe).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);
                case "tobacco":
                    return Json(new { names = brans.PipeAccesories.Where(a => a is Tobacco).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { names = new List<string>() {"None"} }, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetNamePartial(string type, string brand,int? personId = null)
        {
            var person = UserHelper.GetCurentPerson(db, personId);

            var brans = await db.Brands.FindAsync(brand);
            List<PipeAccesory> items = new List<PipeAccesory>();
            switch (type)
            {
                case "bowl":
                    items = brans.PipeAccesories.Where(a => a is Bowl).OrderBy(a => a.AccName).ToList();
                    break;

                case "pipe":
                    items = brans.PipeAccesories.Where(a => a is Pipe).OrderBy(a => a.AccName).ToList();
                    break;
                case "tobacco":
                    items = brans.PipeAccesories.Where(a => a is Tobacco).OrderBy(a => a.AccName).ToList();
                    break;
            }

            var model = new AddPipeAccesoryViewModel();
            model.items = items;
            model.owndItemsId = person.OwnedPipeAccesories.Where(a => a.DeleteDate == null).Select(a => a.PipeAccesoryId).ToList();
            model.accessoryType = type;

            return View("_AddPipeAccesory", model);
        }

        private JsonResult GetBrandFromQuery(IQueryable<Brand> brands)
        {
            return Json(new {brans = brands.Select(a => a.Name).OrderBy(a => a).ToList()},JsonRequestBehavior.AllowGet);
        }
     

    }

    public class AddPipeAccesoryViewModel
    {
        public List<PipeAccesory> items;
        public List<int> owndItemsId;
        public string accessoryType;
        public int amount;
    }
}