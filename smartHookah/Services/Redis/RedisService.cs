namespace smartHookahCommon
{
    using System.Configuration;

    using ServiceStack.Redis;

    using smartHookah.Models.Redis;

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
    }
}