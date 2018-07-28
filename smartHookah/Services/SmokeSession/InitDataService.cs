using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using smartHookah.Models;
using ServiceStack.Common;
using smartHookah.Models.Redis;
using smartHookahCommon;
using System.Threading.Tasks;

namespace smartHookah.Services.SmokeSession
{
    public class InitDataService : IInitDataService
    {
        private readonly SmartHookahContext _db;

        public InitDataService(SmartHookahContext db)
        {
            this._db = db;
        }

        public DynamicSmokeStatistic GetRedisData(string id)
        {
            var result = RedisHelper.GetSmokeStatistic(null, id);
            return result;
        }

        public SmokeSessionStatistics GetStatistics(string id)
        {
            var session = _db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            var result = session?.Statistics;
            return result;
        }

        public SmokeSessionMetaData GetMetaData(string id)
        {
            var session = _db.SmokeSessions.Include(a => a.MetaData).FirstOrDefault(s => s.SessionId == id);
            var result = session?.MetaData;
            return result;
        }

        public HookahSetting GetStandSettings(string id)
        {
            var session = _db.SmokeSessions.Include(a => a.Hookah).Include(a => a.Hookah.Setting).FirstOrDefault(s => s.SessionId == id);
            var result = session?.Hookah.Setting;
            return result;
        }
    }
}