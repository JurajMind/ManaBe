namespace smartHookahCommon
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using log4net;

    using ServiceStack.Redis;

    using smartHookah.Jobs;
    using smartHookah.Migrations;
    using smartHookah.Models;
    using smartHookah.Models.Redis;

    public class RedisService : IRedisService
    {
        private readonly PooledRedisClientManager redisManager;
        private readonly ILog logger = LogManager.GetLogger(typeof(SessionAutoEnd));
        public RedisService()
        {
            this.redisManager = new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        }
        public string GetHookahId(string sessionId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<string>("session:" + sessionId);
            }
        }

        public DynamicSmokeStatistic GetSessionStatistic(string sessionId)
        {
            if (sessionId == null)
            {
                return null;
            }
            using (var redis = this.redisManager.GetClient())
            {
                return redis.As<DynamicSmokeStatistic>()[$"DS:{sessionId}"];
            }
        }

        public void SetSmokeSessionStatistic(string sessionId, DynamicSmokeStatistic ds)
        {
            using (var redis = this.redisManager.GetClient())
            {
                this.setStatistic(sessionId, ds, redis);
            }
        }

        public DynamicSmokeStatistic GetHookahStatistic(string hookahId)
        {
            if (hookahId == null)
            {
                return null;
            }

            var sessionId = this.GetSmokeSessionId(hookahId);
            return GetSessionStatistic(sessionId);
        }

        public void SetHookahStatistic(string hookahId, DynamicSmokeStatistic ds)
        {
            using (var redis = this.redisManager.GetClient())
            {
                var sessionId = this.getSmokeSessionId(hookahId, redis);
                this.setStatistic(sessionId, ds,redis);
            }
        }

        private void setStatistic(string sessionId, DynamicSmokeStatistic ds,IRedisClient client)
        {
            client.Set($"DS:{sessionId}", ds);
        }

        public string GetSmokeSessionId(string hookahId)
        {
            using (var redis = redisManager.GetClient())
            {
                return getSmokeSessionId(hookahId, redis);
            }
        }

        private string getSmokeSessionId(string hookahId, IRedisClient client)
        {
            return client.Get<string>($"hookah:{hookahId}");
        }

        public string InitRedisService(string hookahId, string sessionId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.Set($"hookah:{hookahId}", sessionId);
                redis.Set($"session:{sessionId}", hookahId);
            }

            return sessionId;
        }

        public bool RemoveSession(string sessionId)
        {
            try
            {
                using (var reddis = redisManager.GetClient())
                {
                    var hookahId = reddis.As<string>()[$"session:{sessionId}"];
                    reddis.As<string>().RemoveEntry($"seat:{sessionId}");

                    if (hookahId != null)
                    {
                        reddis.As<string>().RemoveEntry("hookah:" + hookahId);
                    }

                    reddis.As<string>().RemoveEntry($"session:{sessionId}");
                    reddis.RemoveEntry($"DS:{sessionId}");
                    reddis.RemoveAllFromList($"pufs:{sessionId}");
                }
                return true;
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                return false;
            }

        }

        public List<Puf> GetPufs(string sessionId)
        {
            using (var redis = this.redisManager.GetReadOnlyClient())
            {
                var pufs = redis.As<Puf>().Lists[$"pufs:{sessionId}"].GetAll();
                return pufs;
            }
        }

        public void AddPuf(Puf puf,string hookahId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                var sessionId = this.getSmokeSessionId(hookahId, redis);
                puf.SmokeSessionId = sessionId;
                redis.As<Puf>().Lists[$"pufs:{sessionId}"].Add(puf);
            }
        }

        public bool IsLiveSesssion(string sessionId)
        {
            return this.GetHookahId(sessionId) != null;
        }
    }
}