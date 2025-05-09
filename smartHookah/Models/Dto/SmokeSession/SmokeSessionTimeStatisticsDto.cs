﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

        public SmokeSessionTimeStatisticsDto()
        {
            this.DayOfWeekDistribution = new Dictionary<int, int>()
            {
                { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }, { 6, 0 }
            };

            this.SessionStartTimeDistribution = new Dictionary<int, int>();
        }
    }
}