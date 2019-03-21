using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using smartHookah.Models;
    using smartHookah.Support;

    using smartHookahCommon;

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

            if (metadata.HeatManagementId != null)
            {
                result.SelectedHms = metadata.HeatManagement.Id;
                if (result.Pipes.All(a => a.Id != result.SelectedPipe))
                {
                    result.Hmses.Add(db.HeatManagments.Find(result.SelectedHms));
                }
            }

            if (metadata.CoalId != null)
            {
                result.SelectedCoal = metadata.Coal.Id;
                if (result.Coals.All(a => a.Id != result.SelectedCoal))
                {
                    result.Coals.Add(db.Coals.Find(result.SelectedCoal));
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
                    result.Bowl = person.Bowls.Any() ? person.Bowls : db.Bowls.ToList();
                    result.Pipes = person.Pipes.Any() ? person.Pipes : db.Pipes.ToList();
                    result.Hmses = person.HeatManagments.Any() ? person.HeatManagments : db.HeatManagments.ToList();
                    result.Coals = person.Coals.Any() ? person.Coals : db.Coals.ToList();
                }
                else
                {
                    result.Bowl = db.Bowls.ToList();
                    result.Pipes = db.Pipes.ToList();
                    result.Hmses = db.HeatManagments.ToList();
                    result.Coals = db.Coals.ToList();
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

        public int SelectedHms { get; set; }

        public int SelectedCoal { get; set; }

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
        public List<Coal> Coals { get; set; }

        public List<HeatManagment> Hmses { get; set; }

        public int? DbSmokeSessionId { get; set; }
        
        public HeatKeeper HeatKeeper { get; set; }

        public PackType PackType { get; set; }

        public TobacoMetadataModelViewModel TobacoMetadata { get; set; }

        public CoalType CoalType { get; set; }
        public double CoalsCount { get; set; }
        public string TobacoMixName { get; set; }

        public MixLayerMethod LayerMethod { get; set; }
    }
}