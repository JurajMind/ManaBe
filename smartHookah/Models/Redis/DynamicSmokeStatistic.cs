using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Hubs;
using smartHookah.Support;
using smartHookahCommon;
using ServiceStack.Redis;

namespace smartHookah.Models.Redis
{

    public class ClientDynamicSmokeStatistic
    {
        public int PufCount { get; private set; }
        public string LastPuf { get; private set; }
        public string LastPufTime { get; private set; }
        public string SmokeDuration { get; private set; }
        public string LongestPuf { get; private set; }
        public string Start { get; private set; }
        public string Duration { get; private set; }
        public double LongestPufMilis { get; private set; }

        public ClientDynamicSmokeStatistic(DynamicSmokeStatistic ds)
       {
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
    public class DynamicSmokeStatistic
    {
        public static DynamicSmokeStatistic GetStatistic(string sessionId)
        {
            using (var redis = RedisHelper.redisManager.GetClient())
            {
                var ds = redis.As<DynamicSmokeStatistic>()["DS:" + sessionId];
                if (ds == null)
                {
                    ds = new DynamicSmokeStatistic();
                    ds.FullUpdate(redis, sessionId);
                    ds.LastPufs = new List<Puf>();
                }
              
                return ds;
            }

        }

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

        public void FullUpdate(IRedisClient redis, string sessionId)
        {

            var pufs = redis.As<Puf>().Lists["pufs:" + sessionId].GetAll();
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


    }
}