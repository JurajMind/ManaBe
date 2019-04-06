using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PipeAccessoryUsageDto
    {
        [DataMember, JsonProperty("Id")]
        public int Id { get; set; }

        [DataMember, JsonProperty("BrandName")]
        public string BrandName { get; set; }

        [DataMember, JsonProperty("AccName")]
        public string AccName { get; set; }

        [DataMember, JsonProperty("Type")]
        public string Type { get; set; }

        [DataMember, JsonProperty("Used")]
        public int Used { get; set; }

        [DataMember, JsonProperty("Owned")]
        public bool Owned { get; set; }

        [DataMember, JsonProperty("InMix")]
        public int InMix { get; set; }

        /*        public static PipeAccessoryUsageDto FromModel(PipeAccesory model) => model == null
                    ? null
                    : new PipeAccessoryUsageDto
                    {
                        Id = model.Id,
                        AccName = model.AccName,
                        Type = model.GetTypeName(),
                        Used = model.Statistics.Used,
                        BrandName = model.Brand.DisplayName
                    };

                public static IEnumerable<PipeAccessoryUsageDto> FromModelList(IEnumerable<PipeAccesory> model)
                {
                    if (model == null) yield break;
                    foreach (var item in model)
                    {
                        yield return FromModel(item);
                    }
                } */
    }
}