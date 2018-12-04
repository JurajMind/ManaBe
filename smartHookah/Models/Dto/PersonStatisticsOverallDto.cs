using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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