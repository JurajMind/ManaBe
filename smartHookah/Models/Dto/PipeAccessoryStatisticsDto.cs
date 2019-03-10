using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PipeAccessoryStatisticsDto
    {
        [DataMember, JsonProperty("Used")]
        public int Used { get; set; }

        [DataMember, JsonProperty("Weight")]
        public double Weight { get; set; }

        [DataMember, JsonProperty("PipeAccessoryId")]
        public int PipeAccesoryId { get; set; }

        [DataMember, JsonProperty("SmokeDurationTicks")]
        public Int64 SmokeDurationTick { get; set; }

        [DataMember, JsonProperty("PufCount")]
        public double PufCount { get; set; }

        [DataMember, JsonProperty("BlowCount")]
        public double BlowCount { get; set; }

        [DataMember, JsonProperty("SessionDurationTick")]
        public long SessionDurationTick { get; set; }

        [DataMember, JsonProperty("PackType")]
        public PackType PackType { get; set; }

        [DataMember, JsonProperty("Quality")]
        public double Quality { get; set; }

        [DataMember, JsonProperty("Taste")]
        public double Taste { get; set; }

        [DataMember, JsonProperty("Smoke")]
        public double Smoke { get; set; }

        [DataMember, JsonProperty("Overall")]
        public double Overall { get; set; }

        [DataMember, JsonProperty("SmokeTimePercentil")]
        public double SmokeTimePercentil { get; set; }

        [DataMember, JsonProperty("SessionTimePercentil")]
        public double SessionTimePercentil { get; set; }

        public static PipeAccessoryStatisticsDto FromModel(PipeAccesoryStatistics model) => model == null
            ? null
            : new PipeAccessoryStatisticsDto()
            {
                PipeAccesoryId = model.PipeAccesoryId,
                Used = model.Used,
                PackType = model.PackType,
                BlowCount = model.BlowCount,
                Overall = model.Overall,
                PufCount = model.PufCount,
                Quality = model.Quality,
                SessionDurationTick = model.SessionDurationTick,
                SessionTimePercentil = model.SessionTimePercentil,
                Smoke = model.Smoke,
                SmokeDurationTick = model.SmokeDurationTick,
                SmokeTimePercentil = model.SmokeTimePercentil,
                Taste = model.Taste,
                Weight = model.Weight
            };
    }
}