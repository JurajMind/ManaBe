using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using log4net;
using smartHookah.Models.Db;
using smartHookah.Services.Config;
using smartHookah.Services.Messages;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;

namespace smartHookah.Jobs
{
    using System.Collections.Generic;
    using System.Data.Entity;

    using smartHookah.Services.Device;

    public class SessionAutoEnd
    {

        private readonly IIotService iotService;
        private readonly ISmokeSessionBgService smokeSessionService;
        private readonly IRedisService redisService;
        private readonly SmartHookahContext db;
        private readonly ILog logger = LogManager.GetLogger(typeof(SessionAutoEnd));

        private readonly int offlineMulti = 2;

        public SessionAutoEnd() 
        {
            this.iotService = new IotService();
            this.db = new SmartHookahContext();
            var configService = new ConfigService();
            redisService = new RedisService(configService,this.db);
            var emailService = new EmailService();
            var notificationService = new SignalNotificationService(db, redisService, emailService);
            this.smokeSessionService = new SmokeSessionBgService(this.db, redisService, this.iotService);
        }

        public SessionAutoEnd(IIotService iotService, SmartHookahContext db, ISmokeSessionBgService sessionService)
        {
            this.iotService = iotService;
            this.db = db;
            this.smokeSessionService = sessionService;
        }

        private async Task CleanSleep(string hookahCode, bool online, bool autoSleep)
        {
            if (!online)
                return;

            if (autoSleep)
                return;


            var connectionTime = this.redisService.GetConnectionTime(hookahCode);
            if (connectionTime == null)
                return;

            if (connectionTime.Value.AddMinutes(30) > DateTime.UtcNow)
            {
                logger.Info($"Device {hookahCode} is fall asleep");
                await this.iotService.SendMsgToDevice(hookahCode, "slp:");
            }
        }

        public async Task EndSmokeSessions(PerformContext context, bool debug = false)
        {
            
                var stands = await this.db.Hookahs.ToListAsync();
                var onlineStates = await this.iotService.GetOnlineStates(stands.Select(a => a.Code));

            Parallel.ForEach(
                stands,
                async device =>
                    {
                        try
                        {
                            await this.ProceedDevice(context, debug, device, onlineStates);
                            
                        }
                        catch (Exception e)
                        {
                            context.WriteLine($"Error with {device.Code}");
                            context.WriteLine(e.Message);
                        }
                    });
        }

        private async Task ProceedDevice(PerformContext context, bool debug, Hookah hookah, Dictionary<string, bool> onlineStates)
        {
            var stand = hookah.Code;
            var redisSession = this.redisService.GetSessionId(stand);
            var stateExist = onlineStates.TryGetValue(hookah.Code, out var online);

            if (!stateExist)
            {
                context.WriteLine($"{hookah.Code} does not have state");
                return;
            }

            // Hookah dont want auto session end
            if (hookah.AutoSessionEndTime == -1) return;

            // Get curent smoke statistic
            var ds = this.smokeSessionService.GetDynamicStatistic(redisSession,null);

            // No puf was made
            if (ds?.LastPuf == null)
            {
                await this.CleanSleep(stand, online, hookah.AutoSleep);
                return;
            }

            int span;
            if (online) span = hookah.AutoSessionEndTime;
            else span = hookah.AutoSessionEndTime * this.offlineMulti;

            if (ds.LastPufTime.AddMinutes(span) > DateTime.UtcNow) return;

            if (!debug)
            {
                var sessionId = await this.smokeSessionService.EndSmokeSession(redisSession, SessionReport.AutomaticEnd);
                if (online)
                    if (hookah.AutoSleep)
                    {
                        context.WriteLine($"Autosleep {stand}:{sessionId}");
                        await this.iotService.SendMsgToDevice(stand, "slp:");
                    }

                    else
                    {
                        context.WriteLine($"Restart {stand}:{sessionId}");
                        await this.iotService.SendMsgToDevice(stand, "restart:");
                    }
            }
            else
            {
                context.WriteLine($"Auto end debug debug {stand}:{redisSession}");
            }
            return;
        }
    }
}