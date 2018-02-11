using System;
using System.Collections.Generic;
using System.Linq;
using smartHookah.Models;
using smartHookah.Support;

namespace smartHookah.Controllers
{
    public class SmokeHelper
    {
        public static SmokeStatisticViewModel GetSmokeStatistics(List<Puf> pufs)
        {
            var model = new SmokeStatisticViewModel();

            model.Duration = TimeSpan.FromTicks((pufs.Last().Milis - pufs.First().Milis) * 10000);
            model.Start = pufs.First().DateTime;
            model.Pufs = pufs;
            model.CurentState = pufs.Last();
            model.InTimeSpan = pufs.GetDuration(puf => puf.Type == PufType.In).DefaultIfEmpty(new TimeSpan());
            model.OutTimeSpan = pufs.GetDuration(puf => puf.Type == PufType.Out).DefaultIfEmpty(new TimeSpan()); ;
            model.IdleTimeSpan = pufs.GetDuration(puf => puf.Type == PufType.Idle).DefaultIfEmpty(new TimeSpan()); ;

            model.barSize = 0;
            model.SmokeHistogram = model.InTimeSpan.Bucketize(20, out model.barSize);

            return model;
        }

        public static List<List<Puf>> CreateHistogram(List<Puf> pufs, int i)
        {
            var start = pufs.Min(a => a.DateTime);
            var end = start.AddSeconds(i);
            var result = new List<List<Puf>>();
            var bucket = new List<Puf>();
            var orderpufs = pufs.OrderBy(a => a.DateTime).ToArray();
            for (int j = 0; j < orderpufs.Count(); j++)
            {
                var puf = orderpufs[j];
                if (puf.DateTime >= start && puf.DateTime < end)
                {
                    bucket.Add(puf);
                }
                else
                {

                    start = end;
                    end = start.AddSeconds(i);

                    if (bucket.Count > 0 && bucket.Count % 2 == 0 && puf.Type == PufType.Idle)
                    {
                        bucket.Add(puf);
                    }

                    result.Add(bucket);
                    bucket = new List<Puf>();
                    if (puf.Type != PufType.Idle)
                        j--;
                }

            }
            return result;
        }
    }
}