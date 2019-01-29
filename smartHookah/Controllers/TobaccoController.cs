using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using HtmlAgilityPack;

using PagedList;

using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Support;

namespace smartHookah.Controllers
{
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.Configuration.Attributes;

    using smartHookah.Helpers;
    using smartHookah.Models.ViewModel;
    using smartHookah.Services.Person;
    using smartHookah.Services.Redis;

    using smartHookahCommon;

    [Authorize(Roles = "Admin")]
    public class TobaccoController : Controller
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        private readonly IRedisService redisService;

        public TobaccoController(SmartHookahContext db, IPersonService personService, IRedisService redisService)
        {
            this.db = db;
            this.personService = personService;
            this.redisService = redisService;
        }

        // GET: TobaccoSimple
        public async Task<ActionResult> Brand()
        {
            return View(await db.Brands.Where(a => a.Tobacco).ToListAsync());
        }

        public async Task<ActionResult> Index()
        {


            var filterTobacco = FilterTobacco("", 1, ViewBag, filter: "stats");
            ViewBag.Title = Tbc.Tobacco.tobaccoChart;
            ViewBag.Fa = "fa fa-bar-chart";
            return View("_TobacoList", filterTobacco);

        }
        [Authorize]
        public async Task<ActionResult> Smoked()
        {
            var user = personService.GetCurentPersonIQuerable();
            if (user == null)
                return await Index();

            var filterTobacco = FilterTobacco("", 1, ViewBag, filter: "smoked", person: user);
            ViewBag.Title = "Smoked tobacco chart";
            ViewBag.Fa = "fa fa-users";
            return View("_TobacoList", filterTobacco);
        }
        [Authorize]
        public async Task<ActionResult> Owned()
        {
            var user = personService.GetCurentPersonIQuerable();
            if (user == null)
                return await Index();

            var filterTobacco = FilterTobacco("", 1, ViewBag, filter: "owned", person: user);
            ViewBag.Title = "Owned tobacco chart";
            ViewBag.Fa = "fa fa-shopping-basket";
            return View("_TobacoList", filterTobacco);
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> CreateMix()
        {
            var model = new CreateMixViewModel();
            model.TobaccoMetadata = new TobacoMetadataModelViewModel();
            var person = personService.GetCurentPerson();
            model.TobaccoMetadata.TobacoBrands = person.GetPersonTobacoBrand(db);
            if (person != null)
                model.TobaccoMetadata.MyTobacco = person.MyTobacco;
            return View(model);
        }

        [HttpPost]
        [ActionName("CreateMix")]
        [Authorize]
        public async Task<ActionResult> SaveCreateMix(SaveSmokeMetadataModel model)
        {
            var tobacco = GetTobacoFromMetadata(model, db);

            if (tobacco is TobaccoMix)
            {
                await db.SaveChangesAsync();

                return RedirectToAction("Details", "Tobacco", new { id = tobacco.Id });
            }


            return null;
        }

        public async Task<ActionResult> ShowMyMixes(int? id)
        {
            var model = new MyMixesViewModel();
            var person = this.personService.GetCurentPerson(id);

            var tobacco = db.TobaccoMixs.Where(t => t.AuthorId == person.Id).ToList();
            model.Tobacco = tobacco;
            var user = db.Users.First(a => a.PersonId == person.Id);

            model.Name = user.DisplayName;
            return View("MyMixes", model);
        }

        public async Task<ActionResult> ByBrand(string brand, int? page, string sortOrder = null)
        {
            this.redisService.StoreAdress(Request.UserHostAddress, Request.UserHostName);
            ViewBag.Brand = brand;

            var filterTobacco = FilterTobacco(sortOrder, page, ViewBag, filter: $"brand_{brand}");

            return View(filterTobacco);
        }

        // GET: TobaccoSimple/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            this.redisService.StoreAdress(Request.UserHostAddress, Request.UserHostName);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var tobacco = await db.Tobaccos.Include(a => a.Statistics).FirstOrDefaultAsync(a => a.Id == id);

            if (tobacco == null)
                return HttpNotFound();

            var model = new TobaccoDetailViewModel();

            model.Tobacco = tobacco;

            model.SmokeSession =
                db.SmokeSessions.Where(a => a.MetaData != null && a.MetaData.TobaccoId == tobacco.Id)
                    .OrderByDescending(a => a.Statistics.Start)
                    .Take(10);

            var usedinMix = db.Tobaccos.OfType<TobaccoMix>()
                .Where(c => c.Tobaccos.Any(mp => mp.TobaccoId == tobacco.Id));
            model.UsedInMix = usedinMix.Count();

            model.NamedMix = usedinMix.Where(a => a.AccName != null);

            model.Tobacco.Reviews = db.TobaccoReviews.Where(a => a.ReviewedTobaccoId == id).Take(10).ToList();


            model.CanDeleteMix = false;

            if (model.Tobacco is TobaccoMix)
            {
                var mix = model.Tobacco as TobaccoMix;
                var person = personService.GetCurentPerson(mix.AuthorId);

                if (person != null)
                    model.CanDeleteMix = true;
            }

            return View(model);

        }


        public class FilterTobaccoViewModel
        {
            public IPagedList<Tobacco> PagedList { get; set; }

            public string BrandName { get; set; }

            public string SortOrder { get; set; }

            public string Filter { get; set; }
        }

        private FilterTobaccoViewModel FilterTobacco(string sortOrder, int? page,
            dynamic ViewBag = null, string filter = null, string previousSort = null, IQueryable<Person> person = null)
        {

            //            if (person == null)
            //            {
            //                person = this.personService.GetCurentPersonIQuerable();
            //            }
            sortOrder = String.IsNullOrEmpty(sortOrder) ? "smokeduration_desc" : sortOrder;
            ;
            if (previousSort == sortOrder)
            {
                if (previousSort.EndsWith("_desc"))
                {
                    sortOrder = sortOrder.Remove(sortOrder.Length - 5);
                    ;
                }
                else
                {
                    sortOrder = previousSort + "_desc";
                }
            }


            IQueryable<Tobacco> tobacco = FilterTobaccoCondition(filter, person).Include(a => a.Statistics);


            if (ViewBag != null)
            {
                ViewBag.CurrentSort = sortOrder;
                ViewBag.NameSortParm = sortOrder;
            }

            switch (sortOrder.ToLower())
            {
                case "brand":
                    tobacco = tobacco.OrderBy(a => a.BrandName).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "brand_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.BrandName)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;

                case "name":
                    tobacco = tobacco.OrderBy(a => a.AccName).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;

                case "name_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.AccName).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;

                case "used":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.Used).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "used_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.Used)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "smokeduration":
                    tobacco = tobacco.OrderBy(a => a.Statistics.SmokeDurationTick);
                    break;
                case "smokeduration_desc":
                    tobacco = tobacco.OrderByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "pufcount":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.PufCount)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "pufcount_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.PufCount)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "sessionduration":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.SessionDurationTick)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "sessionduration_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.SessionDurationTick)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "quality":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.Quality).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "quality_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.Quality)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "taste":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.Taste).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "taste_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.Taste)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "smoke":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.Smoke).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "smoke_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.Smoke)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "overall":
                    tobacco =
                        tobacco.OrderBy(a => a.Statistics.Overall).ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;
                case "overall_desc":
                    tobacco =
                        tobacco.OrderByDescending(a => a.Statistics.Overall)
                            .ThenByDescending(a => a.Statistics.SmokeDurationTick);
                    break;

            }



            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return new FilterTobaccoViewModel()
            {
                Filter = filter,
                PagedList = tobacco.ToPagedList(pageNumber, pageSize),
                SortOrder = sortOrder
            };
        }

        private IQueryable<Tobacco> FilterTobaccoCondition(string filter, IQueryable<Person> person)
        {

            if (filter != null)
            {
                var filterPart = filter.Split('_');
                if (filterPart.Length < 1)
                {
                    return db.Tobaccos.Where(a => a.AccName != null);
                }
                else
                {
                    switch (filterPart[0])
                    {
                        case "brand":
                            {
                                if (filterPart.Length < 2)
                                    return db.Tobaccos;
                                else
                                {
                                    var brandName = filterPart[1];
                                    return db.Tobaccos.Where(a => a.BrandName == brandName);
                                }

                                break;
                            }

                        case "myMixes":
                            {
                                var curentPerson = this.personService.GetCurentPerson();
                                return db.TobaccoMixs.Where(a => a.AuthorId == curentPerson.Id);
                            }


                        case "stats":
                            return db.Tobaccos.Where(a => a.Statistics != null && a.AccName != null);

                        case "owned":
                            {
                                var ownedTobacco =
                                    person.Include(a => a.OwnedPipeAccesories).First().Tobacco.Select(a => a.Id).ToList();
                                return db.Tobaccos.Where(a => ownedTobacco.Contains(a.Id));
                            }


                        case "smoked":
                            {
                                var smoked =
                                    person.Include(a => a.SmokeSessions)
                                        .First()
                                        .SmokeSessions.Where(a => a.MetaData != null && a.MetaData.Tobacco != null)
                                        .Select(b => b.MetaData.Tobacco)
                                        .Select(a => a.Id).ToList();
                                return db.Tobaccos.Where(a => smoked.Contains(a.Id));
                            }



                            //case "person":
                            //    tobacco =
                            //        db.Persons.FirstOrDefault(a => a.Id == int.Parse(filterPart[1]))
                            //            .SmokeSessions.Where(a => a.MetaData != null && a.MetaData.TobaccoSimple != null)
                            //            .Select(a => a.MetaData.TobaccoSimple).ToList();
                    }
                }
            }
            else
            {
                return db.Tobaccos;
            }
            return db.Tobaccos;
        }


        public async Task<ActionResult> FilterTobaccoAjax(string sortOrder, int? page, string filter,
            string previousSort = null)
        {

            this.redisService.StoreAdress(Request.UserHostAddress, Request.UserHostName);

            var model = FilterTobacco(sortOrder, page, filter: filter, previousSort: previousSort);

            return View("_FilterTobaccoGrid", model);
        }

        // GET: TobaccoSimple/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.BrandId = new SelectList(db.Brands.Where(b => b.Tobacco == true), "Name", "Name");
            return View();
        }

        // POST: TobaccoSimple/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create([Bind(Include = "Id,AccName,BrandName")] Tobacco tobacco)
        {
            if (ModelState.IsValid)
            {
                db.Tobaccos.Add(tobacco);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(tobacco);
        }

        [Authorize(Roles = "Admin")]
        // GET: TobaccoSimple/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var tobacco = await db.Tobaccos.FindAsync(id);
            if (tobacco == null)
                return HttpNotFound();
            return View(tobacco);
        }

        // POST: TobaccoSimple/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Brand")] Tobacco tobacco)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tobacco).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(tobacco);
        }

        // GET: TobaccoSimple/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var tobacco = await db.Tobaccos.FindAsync(id);
            if (tobacco == null)
                return HttpNotFound();
            return View(tobacco);
        }

        // POST: TobaccoSimple/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var tobacco = await db.Tobaccos.FindAsync(id);
            db.Tobaccos.Remove(tobacco);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
                var groupBy = tobacco.GroupBy(g => g.SubCategory + StringExtensions.TrimAllWithInplaceCharArray(g.AccName.ToUpper())).Where(s => s.Count() > 1).ToList();

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
        public async Task<ActionResult> ImportPost(HttpPostedFileBase file)
        {
            var model = new ImportResultModel();

            using (var reader = new StreamReader(file.InputStream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.TrimOptions = TrimOptions.Trim;
                var records = csv.GetRecords<ImportTobacco>();

                foreach (var record in records)
                {
                    try
                    {
                        var displayArray = record.Brand.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
                        var name = new string(displayArray);
                        var brand = this.db.Brands.FirstOrDefault(a => a.DisplayName == record.Brand || a.Name == record.Name);
                        if (brand == null)
                        {

                         

                            brand = new Brand
                                        {
                                            Tobacco = true,
                                            Name = name,
                                            DisplayName = record.Brand
                                        };
                            this.db.Brands.Add(brand);
                            this.db.SaveChanges();
                            model.newBrands.Add(brand);
                        }

                        if (record.Name == null)
                        {
                            continue;
                        }
                        var tobacco = brand.PipeAccesories.EmptyIfNull().FirstOrDefault(a => a.AccName != null && a.AccName.ToUpper() == record.Name.ToUpper());
                        if (tobacco != null)
                        {
                            tobacco.UpdatedAt = DateTimeOffset.UtcNow;
                            this.db.PipeAccesories.AddOrUpdate(tobacco);
                            model.updatedTobacco.Add(tobacco);
                        }
                        else
                        {
                            Tobacco newTobacco = new Tobacco
                                                     {
                                                         BrandName = brand.Name,
                                                         AccName = this.UppercaseFirst(record.Name),
                                                         SubCategory = record.SubType,
                                                         CreatedAt = DateTimeOffset.UtcNow
                                                     };
                            model.resultTobacco.Add(newTobacco);
                            this.db.Tobaccos.AddOrUpdate(newTobacco);
                        }
                        await db.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                     
                    }

                }



            }
            return  View(model);
        }

        

        string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;
            return char.ToUpper(str[0]) + str.Substring(1).ToLower();
        }

        public class ImportTobacco
        {
            [Index(0)]
            public string Brand { get; set; }

            [Index(1)]
            public string SubType { get; set; }

            [Index(2)]
            public string Name { get; set; }
        }


        public static Tobacco GetTobacoFromMetadata(SaveSmokeMetadataModel model, SmartHookahContext db)
        {
            var tobacoMixParts = model.TobacosMix.Where(a => a.TobaccoId != 0).Distinct().ToList();

            if (tobacoMixParts.Count() == 1)
            {
                return db.Tobaccos.Find(tobacoMixParts.First().TobaccoId);
            }
            else
            {
                if (!tobacoMixParts.Any())
                {
                    return null;
                }
                var tobacoMix = new TobaccoMix();
                if (model.TobacoMixId != 0)
                {
                    tobacoMix = db.Tobaccos.Find(model.TobacoMixId) as TobaccoMix;
                }
                else
                {
                    tobacoMix = new TobaccoMix();

                    var person = UserHelper.GetCurentPerson(db);

                    tobacoMix.Author = person;

                    if (person.AssignedBrandId != null)
                    {
                        var mixBrand = db.Brands.FirstOrDefault(a => a.Name == person.AssignedBrandId);
                        if (mixBrand == null)
                        {
                            mixBrand = db.Brands.Find("OwnBrand"); ;
                        }
                        tobacoMix.Brand = mixBrand;
                    }
                    else
                    {
                        tobacoMix.Brand = db.Brands.Find("OwnBrand");
                        tobacoMix.Tobaccos = new List<TobacoMixPart>();
                    }
                }

                tobacoMix.LayerMethod = (MixLayerMethod)model.LayerMethod;
                var mixContinue = true;
                //Named mix
                if (!String.IsNullOrEmpty(tobacoMix.AccName))
                {
                    var dbTopacoMix = tobacoMix.Tobaccos.ToList();
                    var firstNotSecond =
                        dbTopacoMix.Select(a => $"{a.TobaccoId}:{a.Fraction}")
                            .Except(tobacoMixParts.Select(a => $"{a.TobaccoId}:{a.Fraction}"))
                            .ToList();
                    var secondNotFirst =
                        tobacoMixParts.Select(a => $"{a.TobaccoId}:{a.Fraction}")
                            .Except(dbTopacoMix.Select(a => $"{a.TobaccoId}:{a.Fraction}"))
                            .ToList();

                    if (!firstNotSecond.Any() && !secondNotFirst.Any())
                    {
                        mixContinue = false;

                        return tobacoMix;
                    }
                }

                if (mixContinue)
                {
                    var tobacoToRemoveAray = tobacoMix.Tobaccos.ToArray();
                    for (int i = 0; i < tobacoToRemoveAray.Length; i++)
                    {
                        var t = tobacoToRemoveAray[i];
                        db.TobaccosMixParts.Remove(t);
                    }


                    foreach (TobacoMixPart tobacoMixPart in tobacoMixParts)
                    {
                        tobacoMixPart.Tobacco = db.Tobaccos.Find(tobacoMixPart.TobaccoId);

                        tobacoMix.Tobaccos.Add(tobacoMixPart);
                    }
                    if (!String.IsNullOrEmpty(model.TobacoMixName))
                        tobacoMix.AccName = model.TobacoMixName;

                    if (tobacoMix.Tobaccos.Any())
                    {

                        db.Tobaccos.AddOrUpdate(tobacoMix);
                        return tobacoMix;
                    }
                    else
                    {
                        db.Tobaccos.Remove(tobacoMix);
                        return null;
                    }
                }
                return null;
            }
        }

        public async Task<ActionResult> DeleteTobacoMix(int id)
        {
            var tobacoMix = await db.TobaccoMixs.FindAsync(id);


            if (tobacoMix == null)
                return RedirectToAction("Mix", "Tobacco");


            var person = UserHelper.GetCurentPerson(db, tobacoMix.AuthorId);

            if (person == null)
                return RedirectToAction("Mix", "Tobacco");

            var usedInSessions = db.SessionMetaDatas.Where(a => a.TobaccoId == id);

            if (!usedInSessions.Any())
            {
                db.TobaccoMixs.Remove(tobacoMix);

            }
            else
            {
                tobacoMix.Deleted = true;
                db.TobaccoMixs.AddOrUpdate(tobacoMix);
            }
            await db.SaveChangesAsync();

            return RedirectToAction("Mix", "Tobacco");
        }

        public List<TobaccoTaste> ParseTaste(List<string> taste)
        {
            var result = new List<TobaccoTaste>();
            foreach (var t in taste)
            {
                var dbTaste = db.TobaccoTastes.FirstOrDefault(a => a.OriginalName.ToLower() == t.ToLower());

                if (dbTaste == null)
                {
                    dbTaste = new TobaccoTaste() { OriginalName = t };
                }

                if (dbTaste != null)
                    result.Add(dbTaste);
            }
            return result;
        }

        private List<Tobacco> GetTobaco(string baseUrl, string url, Brand brand, string subName, Dictionary<string, List<string>> similar)
        {
            var result = new List<Tobacco>();
            var web = new HtmlWeb();
            var doc = web.Load(baseUrl + url);

            var tobacos = doc.DocumentNode.SelectNodes(@".//div[contains(@class,'padding10px')]");



            if (tobacos != null)
            {

                foreach (var tobaco in tobacos)
                {
                    var tobaccoName = tobaco.SelectSingleNode(".//h3").InnerText.Trim();

                    var chunk = tobaccoName.Split(new[] { '\t' },StringSplitOptions.RemoveEmptyEntries).Where(a => a != "\n").ToArray();

                    if (chunk.Length > 1)
                    {
                        tobaccoName = chunk[0].Trim();
                        if (similar.ContainsKey(chunk[0]))
                        {
                            similar[chunk[0].Trim()].Add(chunk[1].Trim());
                        }
                        else
                        {
                            similar.Add(chunk[0].Trim(), new List<string>(){ chunk[1].Trim() });
                        }
                      
                    }

                    if(tobaccoName == "HookahFloW")
                        continue;
                    var tasteNodes = tobaco.SelectNodes(@".//a[@class='blackfont']");
                    var taste = tasteNodes?.Select(a => a.InnerText).ToList() ?? new List<string>();

                    var newTobacco = new Tobacco
                    {
                        AccName = tobaccoName,
                        Brand = brand,
                        SubCategory = subName,
                        Tastes = ParseTaste(taste),
                       CreatedAt = DateTime.Now,
                       SimilarAccesories =  new List<SimilarAccesories>()
                       
                    };

                    result.Add(newTobacco);
                }
            }

            else
            {

                var subTobaco = doc.DocumentNode.SelectNodes(@"//h3/a");
                if (subTobaco != null)
                    foreach (var subNode in subTobaco)
                    {
                        var sName = subNode.InnerText.Trim().Replace(brand.Name,"");
                        var subUrl = subNode.Attributes["href"].Value;

                        var sub = GetTobaco(baseUrl, subUrl, brand, sName, similar);
                        result.AddRange(sub);
                    }

            }

            return result;
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CleanMix()
        {
            var mix = db.Tobaccos.OfType<TobaccoMix>().Where(a => a.Tobaccos.Any()).ToList();
            mix = mix.Where(a => a.Tobaccos.Any()).ToList();
            // db.Tobaccos.RemoveRange(mix);
            db.SaveChanges();

            return null;

        }
        [Authorize(Roles = "Admin")]
        public ActionResult CalculateAllTobacoStatistic()
        {
            var Session =
                 db.SmokeSessions.Where(a => a.MetaData.Tobacco != null)
                     .Include(x => x.MetaData)
                     .Include(x => x.Statistics);
            var empty = db.PipeAccesoryStatistics.Where(a => a.PipeAccesory == null);

            db.PipeAccesoryStatistics.RemoveRange(empty);

            var usedTobacc = Session.Select(a => a.MetaData.TobaccoId).ToList().Distinct();

            foreach (var tobaccoId in usedTobacc)
            {
                var tobaccoSessionStatistic = Session.Where(a => a.MetaData.TobaccoId == tobaccoId && a.Statistics != null);

                var stats = Calculate(tobaccoSessionStatistic);

                if (stats == null)
                    continue;


                var tobacco = db.Tobaccos.Find(tobaccoId);

                if (String.IsNullOrEmpty(tobacco.AccName))
                    continue;

                stats.PipeAccesoryId = tobacco.Id;

                db.PipeAccesoryStatistics.AddOrUpdate(stats);
                db.SaveChanges();
            }

            return null;
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CalculateStatistic(int id)
        {

            CalculateStatistic(id, db);
            var allTobacoReview = db.PipeAccesoryStatistics.Select(a => new SessionTicks()
            {
                Session = a.SessionDurationTick,
                Smoke = a.SmokeDurationTick
            }).ToList();
            var percentilStat = CalculatePercentil(id, allTobacoReview, db);
            if (percentilStat != null)
                db.PipeAccesoryStatistics.AddOrUpdate(percentilStat);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CalculateAllPercentil()
        {
            var allStats = db.PipeAccesoryStatistics.Select(a => a.PipeAccesoryId);
            var allTobacoReview = db.PipeAccesoryStatistics.Select(a => new SessionTicks()
            {
                Session = a.SessionDurationTick,
                Smoke = a.SmokeDurationTick
            }).ToList();
            foreach (var statId in allStats)
            {
                var stat = CalculatePercentil(statId, allTobacoReview, db);
                if (stat != null)
                    db.PipeAccesoryStatistics.AddOrUpdate(stat);

            }
            db.SaveChanges();
            return null;

        }

        public class SessionTicks
        {
            public long Smoke { get; set; }

            public long Session { get; set; }

        }

        public static PipeAccesoryStatistics CalculatePercentil(int id, List<SessionTicks> allTobaccoReview, SmartHookahContext db)
        {
            var thisStat = db.PipeAccesoryStatistics.Find(id);

            if (thisStat == null)
                return null;

            thisStat.SmokeTimePercentil = Percentile(allTobaccoReview.Select(a => a.Smoke).ToArray(), thisStat.SmokeDurationTick);
            thisStat.SessionTimePercentil = Percentile(allTobaccoReview.Select(a => a.Session).ToArray(), thisStat.SessionDurationTick);

            return thisStat;


        }

        private static double Percentile(long[] sequence, long excelPercentile)
        {
            int less = 0;
            int equal = 0;
            foreach (long item in sequence)
            {
                if (item < excelPercentile)
                    less++;
                else if (item == excelPercentile)
                    equal++;
            }
            return (double)(200 * less + 100 * equal) / (double)(sequence.Length * 2);
        }

        public static void CalculateStatistic(int id, SmartHookahContext db)
        {
            var Session =
                db.SmokeSessions.Where(a => a.MetaData.Tobacco != null)
                    .Include(x => x.MetaData)
                    .Include(x => x.Statistics);

            var tobaccoSessionStatistic = Session.Where(a => a.MetaData.TobaccoId == id && a.Statistics != null);

            var stats = Calculate(tobaccoSessionStatistic);

            if (stats == null)
            {

                db.PipeAccesoryStatistics.Remove(db.PipeAccesoryStatistics.Find(id));
                db.SaveChanges();
                return;
            }


            var tobacco = db.Tobaccos.Find(id);

            stats.PipeAccesoryId = tobacco.Id;

            db.PipeAccesoryStatistics.AddOrUpdate(stats);
            db.SaveChanges();
            return;
        }

        private static PipeAccesoryStatistics Calculate(IEnumerable<SmokeSession> session)
        {
            var result = new PipeAccesoryStatistics();


            var smokeSessions = session as SmokeSession[] ?? session.ToArray();

            if (!smokeSessions.Any())
                return null;
            result.PufCount = smokeSessions.Average(a => a.Statistics.PufCount);
            result.PackType = smokeSessions.GroupBy(i => i.MetaData.PackType).OrderByDescending(j => j.Count()).Select(a => a.Key).First();
            result.BlowCount = smokeSessions.Average(b => b.Pufs.Count(puf => puf.Type == PufType.Out));
            result.SessionDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SessionDuration.TotalSeconds));
            result.SmokeDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SmokeDuration.TotalSeconds));
            result.Used = smokeSessions.Count();
            result.Weight = smokeSessions.Average(a => a.MetaData.TobaccoWeight);

            var smokeSesinReview = smokeSessions.Where(a => a.Review != null).Select(a => a.Review).ToArray();

            if (smokeSesinReview.Any())
            {
                result.Overall = smokeSesinReview.Average(a => a.Overall);
                result.Taste = smokeSesinReview.Average(a => a.Taste);
                result.Smoke = smokeSesinReview.Average(a => a.Smoke);
                result.Quality = smokeSesinReview.Average(a => a.Quality);
            }




            return result;
        }

        public async Task<ActionResult> Mix()
        {
            var model = new MixViewModel();
            model.MixBrands = await db.Brands.Where(a => a.TobaccoMixBrand && a.Name != "OwnBrand").ToListAsync();

            var person = personService.GetCurentPerson();
            if (person != null)
            {
                model.Mixes = db.TobaccoMixs.Where(a => a.AuthorId == person.Id).Take(5).ToList();
            }
            else
            {
                model.Mixes = new List<TobaccoMix>();
            }


            return View(model);
        }

        public async Task<ActionResult> MixBrand(string id)
        {
            var brand = db.Brands.Find(id);

            if (brand == null)
                return RedirectToAction("Mix");

            var model = new MixBrandModel();
            model.Brand = await db.Brands.FindAsync(id);
            model.Tobacco = db.TobaccoMixs.Where(a => a.BrandName == id && !String.IsNullOrEmpty(a.AccName)).ToList();

            return View(model);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }

    public class MixViewModel
    {
        public List<Brand> MixBrands { get; set; }
        public List<TobaccoMix> Mixes { get; internal set; }
    }

    public class CreateMixViewModel
    {
        public TobacoMetadataModelViewModel TobaccoMetadata { get; set; }
    }

    public class TobaccoDetailViewModel
    {
        public Tobacco Tobacco { get; set; }
        public IQueryable<SmokeSession> SmokeSession { get; set; }
        public int UsedInMix { get; set; }
        public IQueryable<TobaccoMix> NamedMix { get; set; }
        public bool CanDeleteMix { get; set; }
    }

    public class MixBrandModel
    {
        public List<TobaccoMix> Tobacco { get; set; }
        public Brand Brand { get; set; }
    }

    public class MyMixesViewModel
    {
        public List<TobaccoMix> Tobacco { get; set; }
        public string Name { get; set; }
    }
}