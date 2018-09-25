using System.Collections.Generic;
using System.Linq;

namespace smartHookah.Models.Dto
{
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

        public static new TobaccoSimpleDto FromModel(Tobacco model)
        {
            var pipeAccesory = PipeAccesorySimpleDto.FromModel(model);
            if (pipeAccesory == null) return null;

            var result = FromModel(pipeAccesory);
            result.SubCategory = model.SubCategory;

            return result;
        }

        public static TobaccoSimpleDto FromModel(PipeAccesorySimpleDto model)
        {
            return new TobaccoSimpleDto
                       {
                           Id = model.Id,
                           BrandName = model.BrandName,
                           BrandId = model.BrandName,
                           Picture = model.Picture,
                           Name = model.Name
                       };
        }
    }
}