using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmartHookahContext db;

        public HomeController(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<ActionResult> VueTest()
        {
            return View();
        }
        public async Task<ActionResult> Index()
        {

            if (UserHelper.GetCurentPerson() != null)
                return RedirectToAction("Index", "Person");



            return View();


        }
        [HttpGet]
        public async Task<ActionResult> GoToSession()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GoToSession(string id)
        {
            var sessionId = id.ToUpper(); 
            var session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);
            if (session == null)
            {
                return RedirectToAction("GoToSession");
            }
            else
            {
                return RedirectToAction("SmokeSession","SmokeSession",new {id = id});
            }
        }

        [Authorize]
        public async Task<ActionResult> Old()
        {
            var model = new IndexMyViewModel();
            //model.Hookah = db.Hookahs.ToList();
            model.Hookah = UserHelper.GetUserStand().ToList();
            model.Online = await IotDeviceHelper.GetState(model.Hookah.Select(a => a.Code).ToList());

            return View(model);
        }
    }

    public class IndexMyViewModel
    {
        public List<Hookah> Hookah { get; set; }
        public List<string> Online { get; set; }
    }

    public class SmokeStatisticViewModel
    {
        public int barSize;
        public TimeSpan Duration { get; set; }
        public List<Puf> Pufs { get; set; }
        public Puf CurentState { get; set; }
        public DateTime Start { get; set; }
        public IEnumerable<TimeSpan> InTimeSpan { get; set; }
        public IEnumerable<TimeSpan> OutTimeSpan { get; set; }
        public IEnumerable<TimeSpan> IdleTimeSpan { get; set; }
        public List<int> HistogramData { get; set; }
        public int BucketSize { get; set; }
        public TimeSpan LastPufDuration { get; set; }
        public DateTime LastPufTime { get; set; }
        public int MaxPresure { get; set; }
        public int LastPresure { get; set; }
        public TimeSpan LongestPuf { get; set; }
        public List<int> SmokeHistogram { get; set; }
        public DynamicSmokeStatistic Dynamic { get; set; }
        public int PufCount { get; set; }
        public TimeSpan SmokingTime { get; set; }
    }


    public class TobacoMetadataModelViewModel
    {
        public bool MyTobacco { get; set; }

        public bool IsTobacoMix { get; set; } = false;
        public List<SmokeMetadataModalTobacoMix> TobacoMix { get; set; } = new List<SmokeMetadataModalTobacoMix>();

        public int TobacoMixId { get; set; }

        public IEnumerable<string> TobacoBrands { get; set; }
        public string TobacoMixName { get; set;} 

        public MixLayerMethod LayerMethod { get;
                set;
            }
    }
    public class SmokeMetadataModalViewModel
    {

        public static SmokeMetadataModalViewModel CreateSmokeMetadataModalViewModel(SmartHookahContext db,
            SmokeSessionMetaData metaData, Person person)
        {
            var result = new SmokeMetadataModalViewModel();
            result.TobacoMetadata = new TobacoMetadataModelViewModel();

            bool myTobacco;
            var tobacoBrands = GetPersonTobacco(db, person, result, out myTobacco);
            ApplyMetadata(db, metaData, result, tobacoBrands, myTobacco);
            result.MetaDataId = metaData.Id;
            return result;
        }

        public static SmokeMetadataModalViewModel CreateSmokeMetadataModalViewModel(SmartHookahContext db,string sessionId,Person person,
           out SmokeSessionMetaData outMetaData)
        {
            var result = new SmokeMetadataModalViewModel();
            result.TobacoMetadata = new TobacoMetadataModelViewModel();
            var hookahId = RedisHelper.GetHookahId(sessionId);

            if (hookahId == null)
            {
                var tmpSession = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);
                if (tmpSession.MetaData == null)
                {
                    InitOldSmokeSession(tmpSession.Hookah.Id, tmpSession.SessionId);
                }
            }
            else
            {
                smartHookah.Controllers.SmokeSessionController.InitSmokeSession(db,hookahId);
            }

            var dbSession = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);
            result.DbSmokeSessionId = dbSession?.Id;
            bool myTobacco;
            var tobacoBrands = GetPersonTobacco(db, person, result, out myTobacco);

            if (dbSession == null)
            {
                outMetaData = new SmokeSessionMetaData();
                if (person.DefaultMetaData != null)
                    ApplyMetadata(db, person.DefaultMetaData, result, tobacoBrands, myTobacco);

                return result;
            }

     

            if (dbSession.MetaData != null)
            {
                var pipeMetadata = dbSession.Hookah.DefaultMetaData;
                SmokeSessionMetaData personMetadata = null;
                if(person!=null)
                 personMetadata = person.DefaultMetaData;

                var resultMetadata = Extensions.Merge(new List<SmokeSessionMetaData>()
                {
                    dbSession.MetaData,
                    pipeMetadata,
                    personMetadata
                },db);

                ApplyMetadata(db, resultMetadata, result, tobacoBrands, myTobacco);
                outMetaData = dbSession.MetaData;
                result.AssignedPersonCount = dbSession.Persons.Count;
                return result;
            }


            outMetaData = new SmokeSessionMetaData();




            return result;
        }


        private static void ApplyMetadata(SmartHookahContext db, SmokeSessionMetaData metadata,
            SmokeMetadataModalViewModel result, List<string> tobacoBrands, bool myTobacco)
        {
            if (metadata.Tobacco != null)
            {
                result.TobacoMetadata = GetTobaccoMetadata(metadata);
                result.TobacoMetadata.TobacoBrands = tobacoBrands;
                result.TobacoMetadata.MyTobacco = myTobacco;
            }

            if (metadata.Bowl != null)
            {
                result.SelectedBowl = metadata.Bowl.Id;
                if (result.Bowl.All(a => a.Id != result.SelectedBowl))
                {
                    result.Bowl.Add(db.Bowls.Find(result.SelectedBowl));
                }
            }


            if (metadata.Pipe != null)
            {
                result.SelectedPipe = metadata.Pipe.Id;
                if (result.Pipes.All(a => a.Id != result.SelectedPipe))
                {
                    result.Pipes.Add(db.Pipes.Find(result.SelectedPipe));
                }
            }


            result.PackType = metadata.PackType;
            result.HeatKeeper = metadata.HeatKeeper;
            result.CoalType = metadata.CoalType;
            result.CoalsCount = metadata.CoalsCount;
       
            result.TobacoWeight = metadata.TobaccoWeight;
            result.AnonymPeopleCount = metadata.AnonymPeopleCount;
        }

        private static List<string> GetPersonTobacco(SmartHookahContext db, Person person, SmokeMetadataModalViewModel result,
            out bool myTobacco)
        {
            var tobacoBrands = person.GetPersonTobacoBrand(db);
            myTobacco = false;
            if (person != null)
            {
                myTobacco = person.MyTobacco;
                result.MyGear = person.MyGear;
                if (person.MyGear)
                {
                    result.Bowl = person.Bowls;
                    result.Pipes = person.Pipes;
                }
                else
                {
                    result.Bowl = db.Bowls.ToList();
                    result.Pipes = db.Pipes.ToList();
                }
            }
            else
            {
                result.Bowl = db.Bowls.ToList();
                result.Pipes = db.Pipes.ToList();
            }

            result.TobacoMetadata.TobacoBrands = tobacoBrands;
            result.TobacoMetadata.MyTobacco = myTobacco;
            return tobacoBrands;
        }

        private static TobacoMetadataModelViewModel GetTobaccoMetadata(SmokeSessionMetaData metadata)
        {
            var result = new TobacoMetadataModelViewModel();
            if (metadata.Tobacco is TobaccoMix)
            {
                var tobacoMix = metadata.Tobacco as TobaccoMix;
                result.TobacoMixName = tobacoMix.AccName;
                result.TobacoMixId = tobacoMix.Id;
                result.TobacoMix = new List<SmokeMetadataModalTobacoMix>();
                var tobacoArray = tobacoMix.Tobaccos.ToArray();
                for (int i = 0; i < tobacoArray.Length; i++)
                {
                    var part = tobacoArray[i];
                    result.TobacoMix.Add(
                        new SmokeMetadataModalTobacoMix()
                        {
                            name = "Part" + 1,
                            Partin = (int) part.Fraction,
                            TobaccoBrand = part.Tobacco.Brand.Name,
                            TobacoFlavor = part.Tobacco.AccName,
                            TobacoId = part.Tobacco.Id,
                            Id = part.Id
                        });
                }
            }
            else
            {
                result.TobacoMix.Add(new SmokeMetadataModalTobacoMix()
                {
                    TobaccoBrand = metadata.Tobacco.Brand.Name,
                    Partin = System.Convert.ToInt32(metadata.TobaccoWeight),
                    TobacoFlavor = metadata.Tobacco.AccName,
                    TobacoId = metadata.Tobacco.Id,
                    name = "Part0"
                });
                result.TobacoMix[0].TobaccoBrand = metadata.Tobacco.Brand.Name;
                result.TobacoMix[0].TobacoFlavor = metadata.Tobacco.AccName;
                result.TobacoMix[0].TobacoId = metadata.Tobacco.Id;
            }

            return result;
        }
        
        public int MetaDataId { get; set; }

        public bool MyGear { get; set; }

        public int AnonymPeopleCount { get; set; }

        public int AssignedPersonCount { get; set; }

        public double TobacoWeight { get; set; }

        public int SelectedPipe { get; set; }

        public int SelectedBowl { get; set; }

        public static void InitOldSmokeSession(int hookahId, string sessionId)
        {
            using (var db = new SmartHookahContext())
            {
                var dbSession = db.SmokeSessions.FirstOrDefault(s => s.SessionId == sessionId);
                if (dbSession.MetaData != null)
                    return;

                dbSession.SessionId = sessionId;
                dbSession.MetaData = new SmokeSessionMetaData();

                var hookah = db.Hookahs.Find(hookahId);
                if (hookah != null)
                    dbSession.Hookah = hookah;

                db.SmokeSessions.AddOrUpdate(dbSession);
                db.SaveChanges();
            }
        }
        public List<Bowl> Bowl { get; set; }
        public List<Pipe> Pipes { get; set; }

        public int? DbSmokeSessionId { get; set; }
        
        public HeatKeeper HeatKeeper { get; set; }

        public PackType PackType { get; set; }

        public TobacoMetadataModelViewModel TobacoMetadata { get; set; }

        public CoalType CoalType { get; set; }
        public double CoalsCount { get; set; }
        public string TobacoMixName { get; set; }

        public MixLayerMethod LayerMethod { get; set; }
    }

    public class SmokeMetadataModalTobacoMix
    {
        public string name { get; set; }
        public string TobaccoBrand { get; set; }
        
        public string TobacoFlavor { get; set; }
        public int TobacoId { get; set; }

        public int Id { get; set; } = 0;

        public int Partin { get; set; }
    }

    public class TypeStatsModel 
    {
        public PufType type { get; set; }
        public IEnumerable<TimeSpan> TimeSpan { get; set; }
        public IEnumerable<Puf> Pufs { get; set; }
    }
}