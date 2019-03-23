using smartHookah.Models.Db;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using smartHookah.Controllers;
using ServiceStack.Redis;
using smartHookah.Models.Redis;
using smartHookah.Services.Config;
using smartHookah.Services.Place;

namespace smartHookah.Services.Redis
{

    public static class RedisKeys
    {
        public const string SessionKey = "session:{0}";
        public const string DeviceKey = "hookah:{0}";
        public const string DynamicSmokeSessionKey = "DS:{0}";
        public const string PufKey = "pufs:{0}";
        public const string UpdateKey = "update:{0}";
        public const string BrandKey = "brand:{0}";
    }

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
                var key = String.Format(RedisKeys.SessionKey, sessionId);
                return TryGetNamespaced<string>(key, redis);
            }
        }

        private T TryGetNamespaced<T>(string key, IRedisClient redis)
        {
            var namespaced = this.GetNamespacedKey(key);
            var result = redis.Get<T>(namespaced);
            if (result != null)
                return result;

            return redis.Get<T>(key);
        }


        public string GetSessionId(string hookahId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                var key = String.Format(RedisKeys.DeviceKey, hookahId);
                return TryGetNamespaced<string>(key, redis);
            }
        }

        public DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId)
        {
            using (var redis = this.redisManager.GetClient())
            {
                var key = String.Format(RedisKeys.DynamicSmokeSessionKey, sessionId);
                return TryGetNamespaced<DynamicSmokeStatistic>(key, redis);
            }
        }

        public void SetDynamicSmokeStatistic(string sessionId,DynamicSmokeStatistic dynamicSmokeStatistic)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.As<DynamicSmokeStatistic>()[GetNamespacedKey(string.Format(RedisKeys.DynamicSmokeSessionKey,sessionId))] = dynamicSmokeStatistic;
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
                var key = String.Format(RedisKeys.PufKey, sessionId);
                var result = redis.As<Puf>().Lists[GetNamespacedKey(key)].GetAll();
                if (result != null || !result.Any())
                    return result;

                return redis.As<Puf>().Lists[key].GetAll();
            }
        }

        public void SetReservationUsage(int placeId, DateTime date, ReservationUsage reservationUsage)
        {
            using (var redis = this.redisManager.GetClient())
            {
                redis.Set(this.GetNamespacedKey($"Reservations:{placeId}_{date:yyyy-MM-dd}"), reservationUsage);
            }
        }

        public ReservationUsage GetReservationUsage(int placeId, DateTime date)
        {
            using (var redis = this.redisManager.GetClient())
            {
                return redis.Get<ReservationUsage>(this.GetNamespacedKey($"Reservations:{placeId}_{date:yyyy-MM-dd}"));
            }
        }

        public bool CleanSmokeSession(string smokeSessionId)
        {
            try
            {
                using (var reddis = redisManager.GetClient())
                {

                    var hookahId = this.GetHookahId(smokeSessionId);
                    reddis.As<string>().RemoveEntry("seat:" + smokeSessionId);
                    reddis.As<string>().RemoveEntry(String.Format(RedisKeys.DeviceKey, hookahId));
                    reddis.As<string>().RemoveEntry(String.Format(RedisKeys.SessionKey, smokeSessionId));
                    reddis.RemoveEntry(String.Format(RedisKeys.DynamicSmokeSessionKey, smokeSessionId));
                    reddis.RemoveAllFromList(String.Format(RedisKeys.PufKey, smokeSessionId));

                    reddis.As<string>().RemoveEntry(GetNamespacedKey("seat:" + smokeSessionId));
                    reddis.As<string>().RemoveEntry(GetNamespacedKey(String.Format(RedisKeys.DeviceKey, hookahId)));
                    reddis.As<string>().RemoveEntry(GetNamespacedKey(String.Format(RedisKeys.SessionKey, smokeSessionId)));
                    reddis.RemoveEntry(GetNamespacedKey(String.Format(RedisKeys.DynamicSmokeSessionKey, smokeSessionId)));
                    reddis.RemoveAllFromList(GetNamespacedKey(String.Format(RedisKeys.PufKey, smokeSessionId)));

                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public bool CreateSmokeSession(string sessionId,string deviceId)
        {
            try
            {
                using (var redis = redisManager.GetClient())
                {

                    redis.Set(GetNamespacedKey(String.Format(RedisKeys.DeviceKey, deviceId)),sessionId);
                    redis.Set(GetNamespacedKey(String.Format(RedisKeys.SessionKey, sessionId)), deviceId);

                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public DateTime? GetConnectionTime(string deviceCode)
        {
            using (var reddis = redisManager.GetClient())
            {
                return this.TryGetNamespaced<DateTime?>(deviceCode, reddis);
            }
        }

        public void SetConnectionTime(string deviceCode)
        {
            using (var redis = redisManager.GetClient())
            {
                var key = $"connTime:{deviceCode}";
                redis.Set<DateTime?>(GetNamespacedKey(key), DateTime.UtcNow);
            }
        }

        public Puf AddPuf(string smokeSessionId, string hookahId, PufType direction, DateTime pufTime, long milis, int presure = 0)
        {
            using (var redis = redisManager.GetClient())
            {
                if (smokeSessionId == null)
                {
                    smokeSessionId = this.GetSessionId(hookahId);
                }

                var puf = new Puf(smokeSessionId, direction, pufTime, milis, presure);
                redis.As<Puf>().Lists[GetNamespacedKey(String.Format(RedisKeys.PufKey, smokeSessionId))].Add(puf);

                return puf;
            }
        }

        public void StoreUpdate(string token,UpdateController.UpdateRedis update)
        {
            using (var redis = redisManager.GetClient())
            {
                var key = GetNamespacedKey(String.Format(RedisKeys.UpdateKey, token));
                redis.As<UpdateController.UpdateRedis>()[key] = update;
                redis.ExpireEntryAt(key, DateTime.Now.AddMinutes(20));
            }
        }

        public UpdateController.UpdateRedis GetUpdate(string token)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.As<UpdateController.UpdateRedis>()[GetNamespacedKey(string.Format(RedisKeys.UpdateKey,token))];
            }
        }

        public IList<string> GetBrands(string prefix)
        {
            var key = String.Format(RedisKeys.DeviceKey, $"{prefix}*");
            using (var redis = redisManager.GetClient())
            {
               
            }

            return null;
        }

        public void StoreBrands(IList<string> brands)
        {
            var key = String.Format(RedisKeys.BrandKey, $"");
            using (var redis = redisManager.GetClient())
            {
                var allBrands =  redis.SearchKeys(GetNamespacedKey(key));
                redis.RemoveAll(allBrands);
                foreach (var brand in brands)
                {
                    var brandKey = String.Format(RedisKeys.BrandKey, brand);
                    redis.Set(GetNamespacedKey(brandKey),brand);
                }

            }
        }

        private string GetNamespacedKey(string key)
        {
            return $"{this.configService.Enviroment}:{key}";
        }
    }
}