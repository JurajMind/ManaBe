using smartHookah.Hubs;
using smartHookah.Models.Db;
using smartHookah.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace smartHookah.Models.Redis
{


    public class DynamicSmokeStatistic
    {
        public int PufCount { get; set; }

        public DateTime LastBlow { get; set; }

        public int AlertBlowCount { get; set; }

        public TimeSpan LastPufDuration { get; set; }

        public DateTime LastPufTime { get; set; }

        public TimeSpan TotalSmokeTime { get; set; }

        public TimeSpan LongestPuf { get; set; }

        public DateTime Start { get; set; }

        public DateTime LastFullUpdate { get; set; }

        public Puf LastPuf { get; set; }

        public List<Puf> LastPufs { get; set; }

        public void FullUpdate(List<Puf> pufs, string sessionId)
        {
            var inTimeSpan = pufs.GetDuration(puf => puf.Type == PufType.In);
            PufCount = pufs.Count(a => a.Type == PufType.In);
            var timeSpans = inTimeSpan as TimeSpan[] ?? inTimeSpan.ToArray();
            if (timeSpans.Any())
            {
                LongestPuf = timeSpans.Max();
                LastPufDuration = timeSpans.Last();
                LastPufTime = pufs.Where(a => a.Type == PufType.Idle).OrderBy(a => a.DateTime).Last().DateTime;
                Start = pufs.First().DateTime;
                LastFullUpdate = DateTime.Now;
                TotalSmokeTime = inTimeSpan.Aggregate((a, b) => a + b);
                LastPufs = new List<Puf>(6);
                LastPufs.AddRange(pufs.OrderBy(a => a.DateTime).Take(6));
            }
            else
            {
                LongestPuf = new TimeSpan();
                LastPufDuration = new TimeSpan();
                LastPufTime = DateTime.Now;
                Start = DateTime.Now;
                LastFullUpdate = DateTime.Now;
                TotalSmokeTime = new TimeSpan();
                LastPufs = new List<Puf>();
            }



        }

        public DynamicSmokeStatistic Update(Puf puf, string session, string deviceId)
        {
            if (puf.Type == PufType.In)
            {
                PufCount++;
            }

            if (puf.Type == PufType.Out)
            {
                AlertBlowCount++;
            }
            if (LastPuf == null)
            {
                LastPuf = puf;
                FakeEnque(puf);
                return this;
            }
            var t = TimeSpan.FromTicks((long)((puf.Milis - LastPuf.Milis) * 10000));
            if (puf.Type == PufType.Idle)
            {
                if (LastPuf.Type == PufType.In)
                {
                    LastPufDuration = t;
                    TotalSmokeTime = TotalSmokeTime + t;
                    LastPufTime = puf.DateTime;
                    if (t > LongestPuf)
                    {
                        LongestPuf = t;
                    }

                }

            }

            LastPuf = puf;
            FakeEnque(puf);

            AllertHub.ProcessAllerts(this, session, deviceId);


            return this;
        }

        private void FakeEnque(Puf puf)
        {
            LastPufs.Add(puf);
            if (LastPufs.Count > 8)
                LastPufs.RemoveAt(0);
        }
    }
}