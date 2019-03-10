using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    using System;
    using System.Collections.Generic;

    using smartHookah.Models;
    using smartHookah.Models.Redis;

    public class SmokeStatisticViewModel
    {
        public int barSize;
        public TimeSpan Duration { get; set; }
        public List<Puf> Pufs { get; set; }
        public Puf CurentState { get; set; }
        public DateTime Start { get; set; }
        public IEnumerable<TimeSpan> InTimeSpan { get; set; }
        public IEnumerable<TimeSpan> OutTimeSpan { get; set; }
        public IEnumerable<TimeSpan> IdleTimeSpan { get; set; }
        public List<int> HistogramData { get; set; }
        public int BucketSize { get; set; }
        public TimeSpan LastPufDuration { get; set; }
        public DateTime LastPufTime { get; set; }
        public int MaxPresure { get; set; }
        public int LastPresure { get; set; }
        public TimeSpan LongestPuf { get; set; }
        public List<int> SmokeHistogram { get; set; }
        public DynamicSmokeStatistic Dynamic { get; set; }
        public int PufCount { get; set; }
        public TimeSpan SmokingTime { get; set; }
    }
}