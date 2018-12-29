namespace smartHookahCommon
{
    using System.Collections.Generic;
    using System.Configuration;

    using smartHookah.Models;

    using ServiceStack.Redis;

    using smartHookah.Models.Redis;
    using smartHookah.Services.Redis;

    public class RedisService : IRedisService
    {
        private readonly PooledRedisClientManager redisManager;

        public RedisService()
        {
            this.redisManager = new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        }

        public string GetHookahId(string sessionId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<string>($"session:{sessionId}");
            }
        }

        public string GetSessionId(string hookahId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<string>($"hookah:{hookahId}");
            }
        }

        public DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<DynamicSmokeStatistic>($"DS:{sessionId}");
                
            }
        }

        public void StoreAdress(string adress, string hostName)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.AddItemToList(adress,hostName);
            }
        }

        public IList<string> GetAdress(string key)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.GetAllItemsFromList(key);
            }
        }

        public IList<Puf> GetPufs(string sessionId)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.As<Puf>().Lists["pufs:" + sessionId].GetAll();
            }
        }
    }
}