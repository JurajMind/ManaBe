using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Console;
using Hangfire.Server;
using log4net;
using smartHookah.Controllers;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookahCommon;

namespace smartHookah.Jobs
{
    using smartHookah.Services.Device;

    public class SessionAutoEnd
    {

        private readonly IIotService iotService;

        private readonly SmartHookahContext db;
        private readonly ILog logger = LogManager.GetLogger(typeof(SessionAutoEnd));

        private readonly int offlineMulti = 2;

        public SessionAutoEnd() : this(new IotService(), new SmartHookahContext())
        {
        }

        public SessionAutoEnd(IIotService iotService, SmartHookahContext db)
        {
            this.iotService = iotService;
            this.db = db;
        }

        private async Task CleanSleep(string hookahCode, bool online, bool autoSleep)
        {
            if (!online)
                return;

            if (autoSleep)
                return;


            var connectionTime = RedisHelper.GetConnectionTime(hookahCode);
            if (connectionTime == null)
                return;

            if (connectionTime.Value.AddMinutes(30) > DateTime.UtcNow)
            {
                logger.Info($"Device{hookahCode} is fall asleap");
                await IotDeviceHelper.SendMsgToDevice(hookahCode, "slp:");
            }
        }

        public async Task EndSmokeSessions(PerformContext context, bool debug = false)
        {
            
                var stands = this.db.Hookahs.ToList();
                var onlineStates = await this.iotService.GetOnlineStates(stands.Select(a => a.Code));
                foreach (var hookah in stands)
                    try
                    {
                        var stand = hookah.Code;
                        var redisSession = RedisHelper.GetSmokeSessionId(stand);
                        var online = onlineStates[hookah.Code];

                        // Hookah dont want auto session end
                        if (hookah.AutoSessionEndTime == -1)
                            continue;

                        // Get curent smoke statistic
                        var ds = DynamicSmokeStatistic.GetStatistic(redisSession);

                        // No puf was made
                        if (ds?.LastPuf == null)
                        {
                            await this.CleanSleep(stand, online, hookah.AutoSleep);
                            continue;
                        }

                        int span;
                        if (online)
                            span = hookah.AutoSessionEndTime;
                        else
                            span = hookah.AutoSessionEndTime * this.offlineMulti;


                        if (ds.LastPufTime.AddMinutes(span) > DateTime.UtcNow)
                            continue;

                        if (!debug)
                        {
                            var sessionId = await SmokeSessionController.EndSmokeSession(redisSession, db, true);
                            if (online)
                                if (hookah.AutoSleep)
                                {
                                    context.WriteLine($"Autosleep {stand}:{sessionId}");
                                    await IotDeviceHelper.SendMsgToDevice(stand, "slp:");
                                }


                                else
                                {
                                    context.WriteLine($"Autosleep {stand}:{sessionId}");
                                    await IotDeviceHelper.SendMsgToDevice(stand, "restart:");
                                }
                        }
                        else
                        {
                            context.WriteLine($"Autoend debug {stand}:{redisSession}");
                        }
                    }
                    catch (Exception e)

                    {
                        context.WriteLine(hookah.Code);
                        context.WriteLine(e);
                    }
            }
        
    }
}