using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public class MetadataModalViewModelMapper : IMetadataModalViewModelMapper
    {
        private readonly IRedisService redisService;
        private readonly SmartHookahContext db;

        public MetadataModalViewModelMapper(IRedisService redisService, SmartHookahContext db)
        {
            this.redisService = redisService;
            this.db = db;
        }

        public SmokeMetadataModalViewModel Map(string sessionId, SmokeSessionMetaData metaData, Models.Db.Person person,
            out SmokeSessionMetaData outMetaData)
        {
            var result = new SmokeMetadataModalViewModel();
            result.TobacoMetadata = new TobacoMetadataModelViewModel();
            var hookahId = redisService.GetHookahId(sessionId);

            var dbSession = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);
            result.DbSmokeSessionId = dbSession?.Id;
            bool myTobacco;
            var tobacoBrands = this.GetPersonTobacco(person, result, out myTobacco);

            if (dbSession == null)
            {
                outMetaData = new SmokeSessionMetaData();
                if (person.DefaultMetaData != null)
                    ApplyMetadata(person.DefaultMetaData, result, tobacoBrands, myTobacco);

                return result;
            }


            if (dbSession.MetaData != null)
            {
                var pipeMetadata = dbSession.Hookah.DefaultMetaData;
                SmokeSessionMetaData personMetadata = null;
                if (person != null)
                    personMetadata = person.DefaultMetaData;

                var resultMetadata = Extensions.Merge(new List<SmokeSessionMetaData>()
                {
                    dbSession.MetaData,
                    pipeMetadata,
                    personMetadata
                }, db);

                ApplyMetadata(resultMetadata, result, tobacoBrands, myTobacco);
                outMetaData = dbSession.MetaData;
                result.AssignedPersonCount = dbSession.Persons.Count;
                return result;
            }


            outMetaData = new SmokeSessionMetaData();


            return result;
        }

        private List<string> GetPersonTobacco(Models.Db.Person person, SmokeMetadataModalViewModel result,
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

        private void ApplyMetadata(SmokeSessionMetaData metadata,
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
    }
}