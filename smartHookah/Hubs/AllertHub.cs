﻿using Microsoft.AspNet.SignalR;
using smartHookah.Models.Db;
using smartHookah.Models.Redis;
using smartHookah.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace smartHookah.Hubs
{
    public class AllertHub
    {
        static List<IAllertTriger> Rules = new List<IAllertTriger>() { new ServiceTrigger() };

        public static void ProcessAllerts(DynamicSmokeStatistic stats, string sesionId, string hookahId)
        {
            foreach (var allertTriger in Rules)
            {
                allertTriger.Trigger(stats, sesionId, hookahId);
            }

        }
    }


    public interface IAllertTriger
    {
        void Trigger(DynamicSmokeStatistic stats, string sesionId, string hookahId);
    }

    public class ServiceTrigger : IAllertTriger
    {
        private static IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        public void Trigger(DynamicSmokeStatistic stats, string sessionId, string hookahId)
        {
            if (stats.LastPuf.Type != PufType.Idle)
                return;

            var lastPufs = stats.LastPufs;
            var blows = lastPufs.Where(a => a.Type == PufType.Out);

            if (blows.Count() < 3)
                return;

            var blowDuration = lastPufs.GetDuration(puf => puf.Type == PufType.Out).DefaultIfEmpty(new TimeSpan());
            var allertCount = blowDuration.Count();

            if (allertCount == 3 && blows.Min(a => a.DateTime).Date < DateTime.Now.AddSeconds(4) && blowDuration.All(a => a.TotalMilliseconds < 2000))
            {
                ClientContext.Clients.Group(sessionId).serviceRequest();
                ClientContext.Clients.Group(hookahId).serviceRequest(hookahId);
                stats.LastPufs = new List<Puf>();
            }

        }
    }
}