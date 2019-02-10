﻿using smartHookah.Models.Redis;

namespace smartHookah.Models.Dto
{
    using System;

    public class DynamicSmokeStatisticDto
    {
        public int PufCount { get; private set; }
        public string LastPuf { get; private set; }
        public string LastPufTime { get; private set; }
        public string SmokeDuration { get; private set; }
        public string LongestPuf { get; private set; }
        public string Start { get; private set; }
        public string Duration { get; private set; }
        public double LongestPufMilis { get; private set; }

        public DynamicSmokeStatisticDto(DynamicSmokeStatistic ds)
        {
            if(ds == null)
                return;

            this.PufCount = ds.PufCount;
            this.LastPuf = ds.LastPufDuration.ToString(@"s\.fff");

            if (ds.LastPufTime.Year > 0001)
            {
                this.LastPufTime = ds.LastPufTime.ToString("dd-MM-yyyy HH:mm:ss");
            }
            else
            {
                this.LastPufTime = ds.LastPufTime.ToString("dd-MM-yyyy HH:mm:ss");
            }

            this.SmokeDuration = ds.TotalSmokeTime.ToString(@"hh\:mm\:ss");
            this.LongestPuf = ds.LongestPuf.ToString(@"s\.fff");
            this.Start = ds.Start.ToString("dd-MM-yyyy HH:mm:ss");
            this.Duration = ((DateTime.UtcNow - ds.Start).ToString(@"hh\:mm\:ss"));
            this.LongestPufMilis = ds.LongestPuf.TotalMilliseconds;
        }
    }

    public class DynamicSmokeStatisticRawDto
    {
        
        public int PufCount { get; private set; }
        public long LastPuf { get; private set; }
        public long LastPufTime { get; private set; }
        public long SmokeDuration { get; private set; }
        public long LongestPuf { get; private set; }
        public long Start { get; private set; }
        public long Duration { get; private set; }
        public long LongestPufMilis { get; private set; }

        public DynamicSmokeStatisticRawDto() { }

        public DynamicSmokeStatisticRawDto(DynamicSmokeStatistic ds)
        {
            if (ds == null)
                return;

            this.PufCount = ds.PufCount;
            this.LastPuf = ds.LastPufDuration.Ticks;

            if (ds.LastPufTime.Year > 0001)
            {
                this.LastPufTime = ds.LastPufTime.Ticks;
            }
            else
            {
                this.LastPufTime = ds.LastPufTime.Ticks;
            }

            this.SmokeDuration = ds.TotalSmokeTime.Ticks;
            this.LongestPuf = ds.LongestPuf.Ticks;
            this.Start = ds.Start.Ticks;
            this.Duration = (DateTime.UtcNow - ds.Start).Ticks;
            this.LongestPufMilis = ds.LongestPuf.Ticks;
        }

        public DynamicSmokeStatisticRawDto(SmokeSessionStatistics statistics)
        {
            this.PufCount = statistics.PufCount;
            this.SmokeDuration = statistics.SmokeDuration.Milliseconds;
            this.LongestPuf = statistics.LongestPuf.Milliseconds;
            this.Start = statistics.Start.Millisecond;
            this.Duration = statistics.SessionDuration.Milliseconds;
            this.LongestPuf = statistics.LongestPuf.Milliseconds;
        }
    }
}