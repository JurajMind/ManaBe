using Newtonsoft.Json;
using smartHookah.Models.Db;
using System;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class SmokeSessionStatisticsDto
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }

        [DataMember]
        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [DataMember]
        [JsonProperty("end")]
        public DateTime End { get; set; }

        [DataMember]
        [JsonProperty("smokeDuration")]
        public TimeSpan SmokeDuration { get; set; }

        [DataMember]
        [JsonProperty("longestPuf")]
        public TimeSpan LongestPuf { get; set; }

        [DataMember]
        [JsonProperty("pufCount")]
        public int PufCount { get; set; }

        [DataMember]
        [JsonProperty("estimatedPersonCount")]
        public int EstimatedPersonCount { get; set; }

        [DataMember]
        [JsonProperty("sessionDuration")]
        public TimeSpan SessionDuration { get; private set; }

        public static SmokeSessionStatisticsDto FromModel(SmokeSessionStatistics model)
        {
            return new SmokeSessionStatisticsDto()
            {
                Id = model.Id,
                Start = model.Start,
                End = model.End,
                SmokeDuration = model.SmokeDuration,
                LongestPuf = model.LongestPuf,
                PufCount = model.PufCount,
                EstimatedPersonCount = model.EstimatedPersonCount,
                SessionDuration = model.SessionDuration,
            };
        }

        public SmokeSessionStatistics ToModel()
        {
            return new SmokeSessionStatistics()
            {
                Id = Id,
                Start = Start,
                End = End,
                SmokeDuration = SmokeDuration,
                LongestPuf = LongestPuf,
                PufCount = PufCount,
                EstimatedPersonCount = EstimatedPersonCount,
            };
        }
    }
}