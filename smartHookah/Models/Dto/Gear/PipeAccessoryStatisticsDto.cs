﻿using Newtonsoft.Json;
using smartHookah.Models.Db;
using System;
using System.Runtime.Serialization;

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

        [DataMember, JsonProperty("Cut")]
        public double Cut { get; set; }

        [DataMember, JsonProperty("Strength")]
        public double Strength { get; set; }


        [DataMember, JsonProperty("Duration")]
        public double Duration { get; set; }

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
                Duration = model.Duration,
                Cut = model.Cut,
                Strength = model.Strength,
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