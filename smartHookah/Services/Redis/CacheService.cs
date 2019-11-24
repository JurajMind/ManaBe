using ServiceStack.Redis;
using smartHookah.Services.Config;
using System;
using System.Configuration;

namespace smartHookah.Services.Redis
{

    public class CacheService : ICacheService
    {
        private readonly PooledRedisClientManager redisManager;

        private readonly IConfigService configService;

        public CacheService(IConfigService configService)
        {
            this.configService = configService;
            this.redisManager = new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        }

        public void Store<T>(string key, T value)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.Set(this.TransformKey(key), value, new TimeSpan(0, 0, 15));
            }
        }

        public T Get<T>(string key)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<T>(this.TransformKey(key));
            }
        }

        public void Invaldate(string key)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.Remove(this.TransformKey(key));
            }
        }

        private string TransformKey(string key)
        {
            return $"{this.configService.Enviroment},{key}";
        }
    }
}