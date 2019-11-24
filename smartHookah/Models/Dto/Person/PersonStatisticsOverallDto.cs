using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PersonStatisticsOverallDto
    {
        [DataMember, JsonProperty("TimeStatistics")]
        public SmokeSessionTimeStatisticsDto SmokeSessionTimeStatistics { get; set; }

        [DataMember, JsonProperty("AccessoriesUsage")]
        public List<PipeAccessoryUsageDto> PipeAccessoriesUsage { get; set; }

        [DataMember, JsonProperty("SmokeSessions")]
        public IEnumerable<SmokeSessionSimpleDto> SmokeSessions { get; set; }
    }
}