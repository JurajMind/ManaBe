namespace smartHookahCommon
{
    using System.Configuration;

    using ServiceStack.Redis;

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
                return redis.Get<string>("session:" + sessionId);
            }
        }
    }
}