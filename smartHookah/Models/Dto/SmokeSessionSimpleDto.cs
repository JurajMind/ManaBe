using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    using smartHookah.Models.Redis;

    [DataContract]
    public class SmokeSessionSimpleDto
    {
        [DataMember, JsonProperty("SessionId")]
        public string SessionId { get; set; }

        [DataMember, JsonProperty("Device")]
        public DeviceSimpleDto Device { get; set; }

        [DataMember, JsonProperty("Statistic")]
        public DynamicSmokeStatisticDto Statistic { get; set; }

        [DataMember, JsonProperty("MetaData")]
        public SmokeSessionMetaDataDto MetaData { get; set; }

        [DataMember, JsonProperty("Place")]
        public PlaceSimpleDto Place { get; set; }

        public static SmokeSessionSimpleDto FromModel(SmokeSession model) => model == null
            ? null
            : new SmokeSessionSimpleDto()
            {
                SessionId = model.SessionId,
                Device = DeviceSimpleDto.FromModel(model.Hookah),
                MetaData = SmokeSessionMetaDataDto.FromModel(model.MetaData),
                Place = PlaceSimpleDto.FromModel(model.Place),
                Statistic = new DynamicSmokeStatisticDto(model.DynamicSmokeStatistic)
            }; 
        

        public static IEnumerable<SmokeSessionSimpleDto> FromModelList(List<SmokeSession> model)
        {
            if (model == null) yield break;

            foreach (var item in model)
                yield return FromModel(item);
        }
    }
}