using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using smartHookah.Migrations;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;

namespace smartHookah.Hubs
{
    public class AllertHub
    {
      
    }


    public interface IAllertTriger
    {
        void Trigger(DynamicSmokeStatistic stats,string sesionId,string hookahId);
    }

    public class ServiceTrigger : IAllertTriger
    {
        private static IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        public void Trigger(DynamicSmokeStatistic stats,string sessionId,string hookahId)
        {
            if (stats.LastPuf.Type != PufType.Idle)
                return;

            var lastPufs = stats.LastPufs;
            var blows = lastPufs.Where(a => a.Type == PufType.Out);

            if(blows.Count() < 3)
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

    public class NotificationService : INotificationService
    {
        static List<IAllertTriger> Rules = new List<IAllertTriger>() { new ServiceTrigger() };

        public void ProcessSmokeStatisticAllert(DynamicSmokeStatistic stats, string sessionId, string hookahId)
        {
            foreach (var allertTriger in Rules)
            {
                allertTriger.Trigger(stats, sessionId, hookahId);
            }
        }
    }

    public interface INotificationService
    {
        void ProcessSmokeStatisticAllert(DynamicSmokeStatistic stats, string sessionId, string hookahId);
    }
}