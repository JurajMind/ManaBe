namespace smartHookah.Models.Dto
{
    public class TobaccoTasteDto
    {
        public string CzName { get; set; }

        public string EngName { get; set; }

        public int Id { get; set; }

        public string OriginalName { get; set; }
    }

    public class TobaccoSimpleSimpleDto : PipeAccesorySimpleDto
    {
        public string SubCategory { get; set; }

        public static TobaccoSimpleSimpleDto FromModel(Tobacco model)
        {
            var pipeAccesory = PipeAccesorySimpleDto.FromModel(model);
            if (pipeAccesory == null) return null;

            var result = FromModel(pipeAccesory);
            result.SubCategory = model.SubCategory;

            return result;
        }

        public static TobaccoSimpleSimpleDto FromModel(PipeAccesorySimpleDto model)
        {
            return new TobaccoSimpleSimpleDto
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