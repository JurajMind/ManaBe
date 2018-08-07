using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using smartHookah.Controllers;
using smartHookah.Controllers.Api;
using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Services.SmokeSession;
using smartHookah.Support;

namespace smartHookahCommon
{
    using System;
    using System.Configuration;
    using ServiceStack.Redis;
    using smartHookah.Models.Redis;
    using static smartHookah.Controllers.UpdateController;

    public class RedisService : IRedisService
    {
        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        private readonly PooledRedisClientManager _redisManager;

        public RedisService()
        {
            this._redisManager =
                new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        }

        public string GetHookahId(string sessionId)
        {
            using (var redis = this._redisManager.GetClient())
            {
                return redis.Get<string>($"session:{sessionId}");
            }
        }

        public DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId)
        {
            using (var redis = this._redisManager.GetClient())
            {
                return redis.Get<DynamicSmokeStatistic>($"DS:{sessionId}");
            }
        }

        public IEnumerable<string> GetSmokeSessionIds(List<string> hookahIds)
        {
            foreach (var id in hookahIds)
            {
                using (var redis = this._redisManager.GetClient())
                {
                    var sessionId = Support.RandomString(5);
                    yield return GetSmokeSessionId(id);
                }
            }
        }

        public string GetSmokeSessionId(string hookahId)
        {
            using (var redis = _redisManager.GetClient())
            {
                var result = redis.Get<string>("hookah:" + hookahId);
                if (result == null) throw new ItemNotFoundException("Hookah not found.");
                return result;
            }
        }
        
        public List<Puf> GetPufs(string sessionId)
        {
            using (var redis = _redisManager.GetClient())
            {
                return redis.As<Puf>().Lists["pufs:" + sessionId].GetAll();
            }
        }

        public void CleanAll()
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.FlushAll();
            }
        }

        public Puf AddPuff(string smokeSessionId, PufType direction, DateTime pufTime)
        {
            using (var reddis = _redisManager.GetClient())
            {
                var puf = new Puf(smokeSessionId, direction, pufTime);
                reddis.As<Puf>().Lists["pufs:" + smokeSessionId].Add(puf);

                return puf;
            }
        }

        public Puf AddPuff(string smokeSessionId, string hookahId, PufType direction, DateTime pufTime, long milis,
            int presure = 0)
        {
            using (var reddis = _redisManager.GetClient())
            {
                if (smokeSessionId == null)
                {
                    smokeSessionId = GetSmokeSessionId(hookahId);
                }

                var puf = new Puf(smokeSessionId, direction, pufTime, milis, presure);
                reddis.As<Puf>().Lists["pufs:" + smokeSessionId].Add(puf);

                return puf;
            }
        }

        public void CreateRedisSession(string hookahId, string sessionId)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.Set("hookah:" + hookahId, sessionId);
                redis.Set("session:" + sessionId, hookahId);
            }
        }
        
        public void AddSeat(string sessionId, string seat)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.Set("seat:" + sessionId, seat);
            }
        }

        public string GetSeat(string sessionId)
        {
            using (var redis = _redisManager.GetClient())
            {
                return redis.Get<string>("seat:" + sessionId);
            }
        }

        public int GetReservationFromTable(string table)
        {
            using (var redis = _redisManager.GetClient())
            {
                return redis.Get<int>("tabRsv:" + table);
            }
        }

        public void SetReservationToTable(string table, int reservation)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.Set($"tabRsv:{table}", reservation);
            }
        }

        public DateTime? GetConnectionTime(string hookahCode)
        {
            using (var redis = _redisManager.GetClient())
            {
                return redis.Get<DateTime?>("connTime:" + hookahCode);
            }
        }

        public void SetConnectionTime(string hookahCode)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.Set<DateTime?>("connTime:" + hookahCode, DateTime.UtcNow);
            }
        }

        public string GetHookahSeat(string hookahCode)
        {
            var sessionId = GetSmokeSessionId(hookahCode);
            if (sessionId == null)
                return null;
            var seat = GetSeat(sessionId);

            return seat;
        }

        public Dictionary<string, string> GetHookahSeat(List<string> hookahCode)
        {
            var result = new Dictionary<string, string>();
            using (var redis = _redisManager.GetClient())
            {
                foreach (var code in hookahCode)
                {
                    var sessionId = GetSmokeSessionId(code);
                    if (sessionId == null)
                        continue;
                    var seat = GetSeat(sessionId);

                    if (seat == null)
                        continue;

                    result.Add(code, seat);
                }

                return result;
            }
        }

        public bool EndSmokeSession(string smokeSessionId)
        {
            try
            {
                using (var reddis = _redisManager.GetClient())
                {
                    var hookahId = reddis.As<string>()["session:" + smokeSessionId];
                    reddis.As<string>().RemoveEntry("seat:" + smokeSessionId);
                    reddis.As<string>().RemoveEntry("hookah:" + hookahId);
                    reddis.As<string>().RemoveEntry("session:" + smokeSessionId);
                    reddis.RemoveEntry("DS:" + smokeSessionId);
                    reddis.RemoveAllFromList("pufs:" + smokeSessionId);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetUpdateFilePath(string id, string token)
        {
            using (var redis = _redisManager.GetClient())
            {
                var updateRedis = redis.As<UpdateController.UpdateRedis>()["Update:" + token];

                if (updateRedis.HookahCode != id)
                    throw new HttpException(404, "File not found");

                return updateRedis.FilePath;
            }
        }

        public void Update(UpdateRedis updateRedis, string updateToken)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.As<UpdateRedis>()["Update:" + updateToken] = updateRedis;
                redis.ExpireEntryAt("Update:" + updateToken, DateTime.Now.AddMinutes(20));
            }
        }

        public void UpdateStatistics(string deviceId, Puf puff)
        {
            using (var redis = _redisManager.GetClient())
            {
                var session = puff.SmokeSessionId;
                var ds = redis.As<DynamicSmokeStatistic>()["DS:" + session];

                if ((ds == null) || (ds.LastFullUpdate < DateTime.Now.AddMinutes(-5)))
                {
                    if (ds == null)
                        ds = new DynamicSmokeStatistic();
                    ds.FullUpdate(redis, session);
                }
                else
                {
                    ds.Update(puff, session, deviceId);
                }

                redis.As<DynamicSmokeStatistic>()["DS:" + session] = ds;

                var oldDs = new
                {
                    pufCount = ds.PufCount,
                    lastPuf = ds.LastPufDuration.ToString(@"s\.fff"),
                    lastPufTime = ds.LastPufTime.AddHours(-1).ToString("dd-MM-yyyy HH:mm:ss"),
                    smokeDuration = ds.TotalSmokeTime.ToString(@"hh\:mm\:ss"),
                    longestPuf = ds.LongestPuf.ToString(@"s\.fff"),
                    start = ds.Start.ToString("dd-MM-yyyy HH:mm:ss"),
                    duration = ((DateTime.UtcNow - ds.Start).ToString(@"hh\:mm\:ss")),
                    longestPufMilis = ds.LongestPuf.TotalMilliseconds
                };

                var ownDs = new DynamicSmokeStatisticDto(ds);

                ClientContext.Clients.Group(session).updateStats(oldDs);
                ClientContext.Clients.Group(deviceId).updateStats(deviceId, ownDs);
            }
        }

        public void OnConnect(string deviceId)
        {
            using (var redis = _redisManager.GetClient())
            {
                redis.As<string>().Lists["Connect:" + deviceId].Add($"{DateTime.Now}");
            }
        }
    }
}