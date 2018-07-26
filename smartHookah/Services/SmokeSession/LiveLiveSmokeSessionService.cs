namespace smartHookah.Services.SmokeSession
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;

    using log4net;

    using Microsoft.AspNet.SignalR;

    using smartHookah.Hubs;
    using smartHookah.Jobs;
    using smartHookah.Models;
    using smartHookah.Models.Redis;
    using smartHookah.Services.Config;
    using smartHookah.Support;

    using smartHookahCommon;

    public class LiveLiveSmokeSessionService : ILiveSmokeSessionService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(LiveLiveSmokeSessionService));

        private readonly IConfigService configService;

        private readonly SmartHookahContext db;

        private readonly INotificationService notificationService;

        private readonly IRedisService redisService;

        private readonly IHubContext smokeSessionHub;



        public LiveLiveSmokeSessionService(
            IRedisService redisService,
            SmartHookahContext db,
            IConfigService configService,
            INotificationService notificationService)
        {
            this.redisService = redisService;
            this.db = db;
            this.configService = configService;
            this.notificationService = notificationService;
            this.smokeSessionHub = GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        }

        public bool AddPuf(Puf puf, string hookahId)
        {
            try
            {
                this.redisService.AddPuf(puf, hookahId);
                this.smokeSessionHub.Clients.Group(puf.SmokeSessionId).pufChange(puf.Type.ToWebStateString(), puf.Type.ToGraphData());
                this.UpdateLiveStatistic(hookahId, puf);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e);
                return false;
            }

        }
   
        public async Task<bool> EndSmokeSession(string smokeSessionId)
        {
            return false;
        }

        public void UpdateLiveStatistic(string hookahId, Puf puf)
        {
            var session = puf.SmokeSessionId;
            var sessionId = this.redisService.GetSmokeSessionId(hookahId);

            var ds = this.redisService.GetSessionStatistic(sessionId);
            if (ds == null || ds.LastFullUpdate
                < DateTime.UtcNow.AddSeconds(this.configService.DynamicSessionUpdateTime))
            {
                if (ds == null) ds = new DynamicSmokeStatistic();
                ds = this.DynamicFullUpdate(ds, sessionId);
            }
            else
            {
                ds = this.DynamicUpdate(ds, puf);
                notificationService.ProcessSmokeStatisticAllert(ds,sessionId,hookahId);
            }

            this.redisService.SetSmokeSessionStatistic(sessionId, ds);

            var ownDs = new ClientDynamicSmokeStatistic(ds);

            this.smokeSessionHub.Clients.Group(session).updateStats(ownDs);
            this.smokeSessionHub.Clients.Group(hookahId).updateStats(hookahId, ownDs);
        }

        public DynamicSmokeStatistic GetSmokeSessionStatistic(string sessionId)
        {
            return this.redisService.GetSessionStatistic(sessionId);
        }

        private DynamicSmokeStatistic DynamicFullUpdate(DynamicSmokeStatistic ds, string sessionId)
        {
            if (ds == null)
            {
                ds = new DynamicSmokeStatistic()
                         {
                             LastPufs = new List<Puf>()
                         };
            }
            var pufs = this.redisService.GetPufs(sessionId);
            var inTimeSpan = pufs.GetDuration(puf => puf.Type == PufType.In);

            ds.PufCount = pufs.Count(a => a.Type == PufType.In);
            var timeSpans = inTimeSpan as TimeSpan[] ?? inTimeSpan.ToArray();
            if (timeSpans.Any())
            {
                ds.LongestPuf = timeSpans.Max();
                ds.LastPufDuration = timeSpans.Last();
                ds.LastPufTime = pufs.Where(a => a.Type == PufType.Idle).OrderBy(a => a.DateTime).Last().DateTime;
                ds.Start = pufs.First().DateTime;
                ds.LastFullUpdate = DateTime.Now;
                ds.TotalSmokeTime = inTimeSpan.Aggregate((a, b) => a + b);
                ds.LastPufs = new List<Puf>(6);
                ds.LastPufs.AddRange(pufs.OrderBy(a => a.DateTime).Take(6));
            }
            else
            {
                ds.LongestPuf = new TimeSpan();
                ds.LastPufDuration = new TimeSpan();
                ds.LastPufTime = DateTime.UtcNow;
                ds.Start = DateTime.Now;
                ds.LastFullUpdate = DateTime.UtcNow;
                ds.TotalSmokeTime = new TimeSpan();
                ds.LastPufs = new List<Puf>();
            }

            return ds;
        }

        private DynamicSmokeStatistic DynamicUpdate( DynamicSmokeStatistic ds, Puf puf)
        {
            if (puf.Type == PufType.In) ds.PufCount++;

            if (puf.Type == PufType.Out) ds.AlertBlowCount++;
            if (ds.LastPuf == null)
            {
                ds.LastPuf = puf;
                ds.LastPufs.Enque(puf, this.configService.DynamicSessionPufCount);
                return ds;
            }

            var t = TimeSpan.FromTicks((puf.Milis - ds.LastPuf.Milis) * 10000);
            if (puf.Type == PufType.Idle)
                if (ds.LastPuf.Type == PufType.In)
                {
                    ds.LastPufDuration = t;
                    ds.TotalSmokeTime = ds.TotalSmokeTime + t;
                    ds.LastPufTime = puf.DateTime;
                    if (t > ds.LongestPuf) ds.LongestPuf = t;
                }

            ds.LastPuf = puf;
            ds.LastPufs.Enque(puf, this.configService.DynamicSessionPufCount);

            return ds;
        }


    }
}