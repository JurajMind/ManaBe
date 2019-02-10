using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class SmokeSessionSimpleDto
    {
        [DataMember, JsonProperty("SessionId")]
        public string SessionId { get; set; }

        [DataMember, JsonProperty("Device")]
        public DeviceSimpleDto Device { get; set; }

        [DataMember, JsonProperty("Statistic")]
        public DynamicSmokeStatisticRawDto Statistic { get; set; }

        [DataMember, JsonProperty("MetaData")]
        public SmokeSessionMetaDataDto MetaData { get; set; }

        [DataMember, JsonProperty("Place")]
        public PlaceSimpleDto Place { get; set; }

        public static SmokeSessionSimpleDto FromModel(SmokeSession model)
        {
            if(model == null)
            {
                return null;
            }
            var statistic = new DynamicSmokeStatisticRawDto();
            if( model.DynamicSmokeStatistic != null)
            {
                statistic = new DynamicSmokeStatisticRawDto(model.DynamicSmokeStatistic);
            }else if(model.Statistics != null)
            {
                statistic = new DynamicSmokeStatisticRawDto(model.Statistics);
            }
            return new SmokeSessionSimpleDto()
            {
                SessionId = model.SessionId,
                Device = DeviceSimpleDto.FromModel(model.Hookah),
                MetaData = SmokeSessionMetaDataDto.FromModel(model.MetaData),
                Place = PlaceSimpleDto.FromModel(model.Place),
                Statistic = statistic
            };
        }
    
            
        

        public static IEnumerable<SmokeSessionSimpleDto> FromModelList(List<SmokeSession> model)
        {
            if (model == null) yield break;

            foreach (var item in model)
                yield return FromModel(item);
        }
    }
}