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
    public class SmokeSessionTimeStatisticsDto
    {
        [DataMember, JsonProperty("SessionsCount")]
        public int SessionsCount { get; set; }

        [DataMember, JsonProperty("SessionsDuration")]
        public TimeSpan SessionsDuration { get; set; }

        [DataMember, JsonProperty("SmokeDuration")]
        public TimeSpan SmokeDuration { get; set; }

        [DataMember, JsonProperty("PuffCount")]
        public int PuffCount { get; set; }

        [DataMember, JsonProperty("DayOfWeekDistribution")]
        public Dictionary<int, int> DayOfWeekDistribution { get; set; }

        [DataMember, JsonProperty("SessionStartTimeDistribution")]
        public Dictionary<int, int> SessionStartTimeDistribution { get; set; }
    }
}