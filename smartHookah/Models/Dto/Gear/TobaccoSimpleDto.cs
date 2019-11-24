using smartHookah.Models.Db;
using System.Collections.Generic;

namespace smartHookah.Models.Dto
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    public class TobaccoTasteDto
    {
        public string CzName { get; set; }

        public string EngName { get; set; }

        public int Id { get; set; }

        public string OriginalName { get; set; }

        public static TobaccoTasteDto FromModel(TobaccoTaste model) => model == null
            ? null
            : new TobaccoTasteDto()
            {
                Id = model.Id,
                CzName = model.CzName,
                EngName = model.EngName,
                OriginalName = model.OriginalName
            };

        public static IEnumerable<TobaccoTasteDto> FromModelList(IEnumerable<TobaccoTaste> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }
    }

    public class TobaccoSimpleDto : PipeAccesorySimpleDto
    {
        public string SubCategory { get; set; }

        public new static TobaccoSimpleDto FromModel(Tobacco model)
        {
            var pipeAccesory = PipeAccesorySimpleDto.FromModel(model);
            if (pipeAccesory == null) return null;

            var result = FromModel(pipeAccesory);
            result.SubCategory = model.SubCategory;

            return result;
        }

        public static TobaccoSimpleDto FromModel(PipeAccesorySimpleDto model) => model == null
            ? null
            : new TobaccoSimpleDto
            {
                Id = model.Id,
                BrandName = model.BrandName,
                BrandId = model.BrandName,
                Picture = model.Picture,
                Name = model.Name
            };

        public static IEnumerable<TobaccoSimpleDto> FromModelList(IEnumerable<Tobacco> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

    }

    public class TobaccoDto : TobaccoSimpleDto
    {
        [DataMember]
        [JsonProperty("Used")]
        public int Used { get; set; }

        public int Duration { get; set; }

        public int PufCount { get; set; }

        public double Rating { get; set; }

        public static new TobaccoDto FromModel(Tobacco model)
        {
            var tobaccoDto = TobaccoSimpleDto.FromModel(model);
            return new TobaccoDto
            {
                Id = tobaccoDto.Id,
                BrandName = tobaccoDto.BrandName,
                BrandId = tobaccoDto.BrandName,
                Picture = tobaccoDto.Picture,
                Name = tobaccoDto.Name,
                PufCount = (int)(model?.Statistics?.PufCount ?? 0),
                Used = model.Statistics?.Used ?? 0,
                Duration = (int)(model.Statistics?.SmokeDurationTick ?? 0)

            };
        }
    }
}