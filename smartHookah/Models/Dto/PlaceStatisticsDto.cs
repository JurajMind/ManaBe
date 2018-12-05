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
    public class PlaceStatisticsDto
    {
        [DataMember, JsonProperty("DayDistribution")]
        public PlotData DayDistribution { get; set; }

        [DataMember, JsonProperty("TimeDistribution")]
        public PlotData TimeDistribution { get; set; }

        [DataMember, JsonProperty("GroupSize")]
        public PlotData GroupSize { get; set; }

        [DataMember, JsonProperty("VisitDuration")]
        public PlotData VisitDuration { get; set; }

        [DataMember, JsonProperty("WeekVisits")]
        public PlotData WeekVisits { get; set; }

        [DataMember, JsonProperty("MonthVisits")]
        public PlotData MonthVisits { get; set; }

        [DataMember, JsonProperty("Customers")]
        public int Customers { get; set; }

        [DataMember, JsonProperty("From")]
        public string From { get; set; }

        [DataMember, JsonProperty("To")]
        public string To { get; set; }

        [DataMember, JsonProperty("DataSpan")]
        public string DataSpan { get; set; }

        [DataMember, JsonProperty("ReservationCount")]
        public int ReservationCount { get; set; }

        [DataMember, JsonProperty("TableUsage")]
        public PlotData TableUsage { get; set; }
    }

    [DataContract]
    public class PlotData
    {
        public PlotData(List<string> labels, List<int> data)
        {
            this.Labels = labels;
            this.Data = data;
        }

        [DataMember, JsonProperty("Labels")]
        public List<string> Labels { get; set; }

        [DataMember, JsonProperty("Data")]
        public List<int> Data { get; set; }
    }
}