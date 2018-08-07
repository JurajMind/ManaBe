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
    public class SessionAutoEnd
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SessionAutoEnd));
        private readonly IRedisService _redisService;
        private readonly int offlineMulti = 2;

        public SessionAutoEnd()
        {//TODO redisService jako parameter konstuktoru vyzadovan v Startup.cs
            _redisService = new RedisService();
        }

        private async Task CleanSleep(string hookahCode, bool online, bool autoSleep)
        {
            if (!online)
                return;

            if (autoSleep)
                return;


            var connectionTime = _redisService.GetConnectionTime(hookahCode);
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
            using (var db = new SmartHookahContext())
            {
                var stands = db.Hookahs.ToList();


                foreach (var hookah in stands)
                    try
                    {
                        var stand = hookah.Code;
                        var redisSession = _redisService.GetSmokeSessionId(stand);
                        var online = await IotDeviceHelper.GetState(stand);
                        // Hookah dont want auto session end
                        if (hookah.AutoSessionEndTime == -1)
                            continue;

                        // Get curent smoke statistic
                        var ds = DynamicSmokeStatistic.GetStatistic(redisSession, _redisService);

                        // No puf was made
                        if (ds?.LastPuf == null)
                        {
                            await CleanSleep(stand, online, hookah.AutoSleep);
                            continue;
                        }

                        int span;
                        if (online)
                            span = hookah.AutoSessionEndTime;
                        else
                            span = hookah.AutoSessionEndTime * offlineMulti;


                        if (ds.LastPufTime.AddMinutes(span) > DateTime.UtcNow)
                            continue;

                        if (!debug)
                        {
                            var sessionId = await SmokeSessionController.EndSmokeSession(redisSession, db, _redisService, true);
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
}