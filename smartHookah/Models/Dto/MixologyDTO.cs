using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    using smartHookah.Support;

    public class MixCreatorsDto
    {
        public ICollection<MixCreator> MixCreatorsList { get; set; }

        public MixCreatorsDto()
        {
            this.MixCreatorsList = new List<MixCreator>();
        }
    }
    

    [DataContract]
    public class TobaccoMixSimpleDto : TobaccoSimpleDto
    {
        [DataMember, JsonProperty("Tobaccos")]
        public ICollection<TobaccoInMix> Tobaccos { get; set; }

        public TobaccoMixSimpleDto()
        {
            this.Tobaccos = new List<TobaccoInMix>();
        }

        public static IEnumerable<TobaccoMixSimpleDto> FromModelList(ICollection<TobaccoMix> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public static TobaccoMixSimpleDto FromModel(TobaccoMix model)
        {
            return model == null
                ? null
                : new TobaccoMixSimpleDto
                {
                    Name = model.AccName,
                    Id = model.Id,
                    BrandName = model.Brand.Name,
                    BrandId = model.BrandName,
                    Type = "TobaccoMix",
                    Tobaccos = model.Tobaccos.Select(
                        a => new TobaccoInMix()
                        {
                            Fraction = a.Fraction,
                            Tobacco = FromModel(a.Tobacco)
                        }).ToList()
                };
        }

        public static TobaccoMix ToModel(TobaccoMixSimpleDto model)
        {
            return model == null
                       ? null
                       : new TobaccoMix
                             {
                                 Id = model.Id,
                                 AccName = model.Name,
                                 BrandName = model.BrandId,
                                 Tobaccos = model.Tobaccos.EmptyIfNull().Select(
                                     t => new TobacoMixPart
                                              {
                                                  Fraction = t.Fraction,
                                                  TobaccoId = t.Tobacco.Id
                                     }).ToList()
                             };
        }
    }

    public class TobaccoInMix
    {
        public TobaccoInMix()
        {
            Tobacco = new TobaccoSimpleDto();
        }

        public TobaccoSimpleDto Tobacco { get; set; }
        public int Fraction { get; set; }

        public static TobaccoInMix FromModel(TobacoMixPart tobacoMixPart)
        {
            return new TobaccoInMix()
            {
                Fraction = tobacoMixPart.Fraction,
                Tobacco = TobaccoSimpleDto.FromModel(tobacoMixPart.Tobacco)
            };
        }
    }

    public class MixCreator
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Picture { get; set; }
        public int MixCount { get; set; }
    }
}