using smartHookah.Models.Db;

namespace smartHookah.Services.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using ServiceStack.Redis;

    using smartHookah.Models;
    using smartHookah.Models.Redis;
    using smartHookah.Services.Config;
    using smartHookah.Services.Place;

    public class RedisService : IRedisService
    {
        private readonly PooledRedisClientManager redisManager;

        private readonly IConfigService configService;

        public RedisService(IConfigService configService)
        {
            this.configService = configService;
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
                redis.AddItemToList(adress, hostName);
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
            using (var redis = this.redisManager.GetClient())
            {
                return redis.As<Puf>().Lists["pufs:" + sessionId].GetAll();
            }
        }

        public void SetReservationUsage(int placeId, DateTime date, ReservationUsageDto reservationUsage)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.Set(this.GetNamespacedKey($"Reservations:{placeId}_{date:yyyy-MM-dd}"), reservationUsage);
            }
        }

        public ReservationUsageDto GetReservationUsage(int placeId, DateTime date)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<ReservationUsageDto>(this.GetNamespacedKey($"Reservations:{placeId}_{date:yyyy-MM-dd}"));
            }
        }

        private string GetNamespacedKey(string key)
        {
            return $"{this.configService.Enviroment}:{key}";
        }
    }
}