using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using ServiceStack.Redis;

namespace smartHookahCommon
{
    public class RedisHelper
    {
        public static PooledRedisClientManager redisManager = new PooledRedisClientManager(ConfigurationManager.AppSettings["RedisConnectionString"]);
        public static string GetSmokeSessionId(string hookahId)
        {
            using (var redis = redisManager.GetClient())
            {
                return GetSmokeSessionId(hookahId, redis);
            }
        }

        public static IEnumerable<string> GetSmokeSessionIds(List<string> hookahIds)
        {
            foreach (var id in hookahIds)
            {
                using (var redis = redisManager.GetClient())
                {
                    yield return GetSmokeSessionId(id, redis);
                }
            }
       
        }

        public static string GetSmokeSessionId(string hookahId, IRedisClient redis)
        {
            var result = redis.Get<string>("hookah:" + hookahId);

            if (result == null)
            {
                result = CreateSmokeSession(hookahId, redis);
            }

            return result;
        }
        

        public static List<Puf> GetPufs(string sessionId)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.As<Puf>().Lists["pufs:" + sessionId].GetAll();
            }
        }

        public static void CleanAll()
        {
            using (var redis = redisManager.GetClient())
            {
                redis.FlushAll();
            }
        }

        public static string GetHookahId(string sessionId)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.Get<string>("session:" + sessionId);
            }
        }

        public static Puf AddPuff(string smokeSessionId, PufType direction, DateTime pufTime)
        {
            using (var reddis = redisManager.GetClient())
            {
                var puf = new Puf(smokeSessionId, direction, pufTime);
                reddis.As<Puf>().Lists["pufs:" + smokeSessionId].Add(puf);

                return puf;
            }
            return null;
        }

        public static Puf AddPuff(string smokeSessionId,string hookahId, PufType direction, DateTime pufTime, long milis,int presure = 0)
        {
            using (var reddis = redisManager.GetClient())
            {
                if (smokeSessionId == null)
                {
                    smokeSessionId = GetSmokeSessionId(hookahId, reddis);
                }

                var puf = new Puf(smokeSessionId, direction, pufTime, milis,presure);
                reddis.As<Puf>().Lists["pufs:" + smokeSessionId].Add(puf);

                return puf;
            }
        }

        



        private static string CreateSmokeSession(string hookahId, IRedisClient redis)
        {
            var sessionId = Support.RandomString(5);
            using (var db = new SmartHookahContext())
            {
                var duplicate = FindDuplicate(db,sessionId);
                while (duplicate)
                {
                    sessionId = Support.RandomString(5);
                    duplicate = FindDuplicate(db, sessionId);
                }
                SmokeSessionController.InitSmokeSession(db, hookahId, sessionId);
            }
           


            redis.Set("hookah:" + hookahId, sessionId);
            redis.Set("session:" + sessionId, hookahId);

          

            return sessionId;
        }

        private static bool FindDuplicate(SmartHookahContext db, string sessionId)
        {
            return db.SmokeSessions.Count(a => a.SessionId == sessionId) > 0;
        }

        public static void AddSeat(string sessionId, string seat)
        {
            using (var redis = redisManager.GetClient())
            {
                redis.Set("seat:" + sessionId, seat);
            }
        }

        public static string GetSeat(string sessionId)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.Get<string>("seat:" + sessionId);
            }
        }

        public static int GetReservatopnFromTable(string table)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.Get<int>("tabRsv:" + table);
            }
        }

        public static void SetReservationToTable(string table,int reservation)
        {
            using (var redis = redisManager.GetClient())
            {
                redis.Set($"tabRsv:{table}", reservation);
            }
        }

        public static DateTime? GetConnectionTime(string hookahCode)
        {
            using (var redis = redisManager.GetClient())
            {
                return redis.Get<DateTime?>("connTime:" + hookahCode);
            }
        }

        public static void SetConnectionTime(string hookahCode)
        {
            using (var redis = redisManager.GetClient())
            {
                redis.Set<DateTime?>("connTime:" + hookahCode,DateTime.UtcNow);
            }
        }

        public static string GetHookahSeat(string hookahCode)
        {
           
                var sessionId = GetSmokeSessionId(hookahCode);
                if (sessionId == null)
                    return null;
                var seat = GetSeat(sessionId);

                return seat;
            
        }

        public static Dictionary<string, string> GetHookahSeat(List<string> hookahCode)
        {
            var result = new Dictionary<string,string>();
            using (var redis = redisManager.GetClient())
            {
                foreach (var code in hookahCode)
                {
                    var sessionId = GetSmokeSessionId(code);
                    if(sessionId == null)
                        continue;
                    var seat = GetSeat(sessionId);

                    if(seat == null)
                        continue;

                    result.Add(code,seat);
                }

                return result;
            }
        }

        public static DynamicSmokeStatistic GetSmokeStatistic(string hookahId = null, string sessionId = null)
        {
            var result = new DynamicSmokeStatistic();
            using (var redis = redisManager.GetClient())
            {
                if (sessionId == null)
            {
                if (hookahId == null)
                    return result;

                sessionId = RedisHelper.GetSmokeSessionId(hookahId,redis);
                if (sessionId == null)
                    return result;
            }

            var ds = redis.As<DynamicSmokeStatistic>()["DS:" + sessionId];

                return ds;
           }
        }

        public static bool EndSmokeSession(string smokeSessionId)
        {
            try
            {
                using (var reddis = redisManager.GetClient())
                {
                    var hookahId = reddis.As<string>()["session:" + smokeSessionId];
                    reddis.As<string>().RemoveEntry("seat:" + smokeSessionId);
                    reddis.As<string>().RemoveEntry("hookah:" + hookahId);
                    reddis.As<string>().RemoveEntry("session:" + smokeSessionId);
                    reddis.RemoveEntry("DS:" + smokeSessionId);
                    reddis.RemoveAllFromList("pufs:"+smokeSessionId);

                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }

        }
    }
}
