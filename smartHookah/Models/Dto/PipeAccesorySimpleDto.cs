using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PipeAccesorySimpleDto
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }
         
        [DataMember]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("Brand")]
        public string BrandName { get; set; }

        [DataMember]
        [JsonProperty("BrandId")]
        public string BrandId { get; set; }

        [DataMember]
        [JsonProperty("Picture")]
        public string Picture { get; set; }

        [DataMember]
        [JsonProperty("Type")]
        public string Type { get; set; }

        public static PipeAccesorySimpleDto FromModel(PipeAccesory model) => model == null ? null : new PipeAccesorySimpleDto()
        {
            Id = model.Id,
            BrandName = model.Brand.DisplayName,
            BrandId = model.BrandName,
            Picture = model.Picture,
            Name = model.AccName,
            Type = model.GetTypeName()
        }; 


        public static PipeAccesorySimpleDto FromModel(Bowl model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Bowl";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Pipe model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Hookah";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Tobacco model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Tobacco";
            return result;
        }

    }
}