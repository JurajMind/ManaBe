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
    using System.IO;

    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.Configuration.Attributes;

    using Google.Apis.Services;

    using smartHookah.Models.Factory;
    using smartHookah.Models.ViewModel;
    using smartHookah.Support;

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

                    case "heatmanagement":
                        {
                            accesory = new HeatManagment(accesory);
                            break;
                        }

                    case "coal":
                        {
                            accesory = new Coal(accesory);
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
                    Person person;
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
                case "heatmanagement":
                    return GetBrandFromQuery(brans.Where(a => a.HeatManagment));
                case "coal":
                    return GetBrandFromQuery(brans.Where(a => a.Coal));
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
                    return Json(new { names = brans.PipeAccessories.Where(a => a is Bowl).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);

                case "pipe":
                    return Json(new { names = brans.PipeAccessories.Where(a => a is Pipe).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);
                case "tobacco":
                    return Json(new { names = brans.PipeAccessories.Where(a => a is Tobacco).Select(a => a.AccName) }, JsonRequestBehavior.AllowGet);
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
                    items = brans.PipeAccessories.Where(a => a is Bowl).OrderBy(a => a.AccName).ToList();
                    break;

                case "pipe":
                    items = brans.PipeAccessories.Where(a => a is Pipe).OrderBy(a => a.AccName).ToList();
                    break;
                case "tobacco":
                    items = brans.PipeAccessories.Where(a => a is Tobacco).OrderBy(a => a.AccName).ToList();
                    break;

                case "heatmanagement":
                    items = brans.PipeAccessories.Where(a => a is HeatManagment).OrderBy(a => a.AccName).ToList();
                    break;

                case "coal":
                    items = brans.PipeAccessories.Where(a => a is Coal).OrderBy(a => a.AccName).ToList();
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

        [HttpGet]
        public ActionResult Import()
        {
            return this.View();
        }

        [HttpGet]
        public ActionResult RemovedDuplicity(string id)
        {

            var brands = this.db.Brands.Where(b => !b.TobaccoMixBrand).Select(s => s.Name);
            foreach (var brand in brands)
            {
                var tobacco = this.db.Tobaccos.Where(p => p.BrandName == brand);
                var groupBy = tobacco.GroupBy(g => g.SubCategory + g.AccName.ToUpper().Trim()).Where(s => s.Count() > 1).ToList();

                try
                {
                    foreach (var item in groupBy)
                    {
                        var minId = item.Min(s => s.Id);
                        this.db.Tobaccos.RemoveRange(item.Where(i => i.Id != minId));
                        this.db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                }


            }
            return null;

        }

        [HttpPost]
        public async Task<ActionResult> ImportPost(HttpPostedFileBase file, string type,string save = "")
        {
            var model = new ImportResultModel();
            var preview = save == "save";
            var importInformation = new ImportInformation
            {
                DataSource = file.FileName,
                DateTimeCreatedAt = DateTime.UtcNow,
                ImportedAccesories = new List<PipeAccesory>()
            };
            using (var reader = new StreamReader(file.InputStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.TrimOptions = TrimOptions.Trim;
            
                var records = csv.GetRecords<ImportPipeAccesory>();
               
                foreach (var record in records)
                {
                    try
                    {
                        var displayArray = record.Brand.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
                        var name = new string(displayArray);
                        var brand = this.db.Brands.FirstOrDefault(a => a.DisplayName == record.Brand || a.Name == record.Brand || a.Name == name);


                        if (brand == null)
                        {
                            brand = BrandFactory.FromFactory(record.Brand, type);
                            if (!preview)
                            {
                                this.db.Brands.Add(brand);
                                this.db.SaveChanges();
                                model.newBrands.Add(brand);
                            }
                        }
                        else
                        {
                            var newBrand = BrandFactory.SetFlag(brand, type);
                            if (!preview)
                            {
                                this.db.Brands.AddOrUpdate(newBrand);
                                this.db.SaveChanges();
                            }

                        }

                        if (record.Name == null)
                        {
                            continue;
                        }
                        var pipeAccesory = brand.PipeAccessories.EmptyIfNull().FirstOrDefault(a => a.AccName != null && a.AccName.ToUpper() == record.Name.ToUpper());
                        if (pipeAccesory != null)
                        {
                            importInformation.ImportedAccesories.Add(pipeAccesory);
                            pipeAccesory.UpdatedAt = DateTimeOffset.UtcNow;
                            this.db.PipeAccesories.AddOrUpdate(pipeAccesory);
                            model.updateImport.Add(pipeAccesory);
                        }
                        else
                        {
                            PipeAccesory newPipeAccesory = PipeAccesoryFactory.CreateFromRecort(record, brand, type);
                            importInformation.ImportedAccesories.Add(newPipeAccesory);
                            model.newImport.Add(newPipeAccesory);
                            if (!preview)
                            {
                                this.db.PipeAccesories.AddOrUpdate(newPipeAccesory);
                            }
                        }
                        if (!preview)
                        {
                            await this.db.SaveChangesAsync();
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);

                    }
                }

            }
            return View(model);
        }

        public ActionResult DataClean()
        {
            var tobaccoInMix = this.db.TobaccosMixParts.Select(s => s.TobaccoId);
            var tobaccoInSession = this.db.SmokeSessions.Where(s => s.MetaData != null && s.MetaData.Tobacco != null)
                .Select(s => s.MetaData.Tobacco.Id);

            var allIds = tobaccoInMix.Union(tobaccoInSession);

            var tobaccos = this.db.Tobaccos.Where(s => allIds.Contains(s.Id));

            var allBrands = tobaccos.Select(b => b.Brand).ToList();

            var allBrandName = allBrands.Select(b => b.Name);
            var brandsLeft = this.db.Brands.Where(b => !allBrandName.Contains(b.Name)).ToList().Where(
                a => a.Tobacco == true && a.Hookah == false && a.Bowl == false && a.TobaccoMixBrand == false
                     && a.Coal == false && a.HeatManagment == false);


            var bb = brandsLeft.ToList();

            this.db.Brands.RemoveRange(brandsLeft);


            this.db.SaveChanges();
            return null;
        }

        public ActionResult GoogleCheck(string id = "Tobacco")
        {
            var accesory = this.db.PipeAccesories.Where(a => a is Tobacco && a.ControlSearch == null && a.CreatedAt == null && a.UpdatedAt == null).Take(100);
            string apiKey = "AIzaSyCf57DrGRdV3LwOGkPfiW0ahi-N2LNyZjQ";
            string cx = "014066877753874603507:loehrtgydoe";

            foreach (var pipeAccesory in accesory)
            {
                var svc = new Google.Apis.Customsearch.v1.CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });
                var query = $"\"{pipeAccesory.Brand.DisplayName} {pipeAccesory.AccName}\"";
                var listRequest = svc.Cse.List(query);

                listRequest.Cx = cx;
                var search = listRequest.Execute();
                var searchResultCount = search.SearchInformation.TotalResults;
                pipeAccesory.ControlSearch = Convert.ToInt32(searchResultCount);
                db.PipeAccesories.AddOrUpdate(pipeAccesory);

            }

            db.SaveChanges();
            return null;
        }

    }

    public class AddPipeAccesoryViewModel
    {
        public List<PipeAccesory> items;
        public List<int> owndItemsId;
        public string accessoryType;
        public int amount;
    }

    public class ImportPipeAccesory
    {
        [Index(0)]
        public string Brand { get; set; }

        [Index(1)]
        public string SubType { get; set; }

        [Index(2)]
        public string Name { get; set; }
    }
}