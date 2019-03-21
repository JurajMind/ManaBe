using System.Configuration;
using ServiceStack.Redis;

namespace smartHookah.Helpers
{
    public class RedisHelper
    {
        private static PooledRedisClientManager redisManager = new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        
        public static void CleanAll()
        {
            using (var redis = redisManager.GetClient())
            {
                redis.FlushAll();
            }
        }
    }
}
