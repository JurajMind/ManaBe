using System.Data.Entity;
using System.Linq;

using smartHookah.Models;
using smartHookah.Models.Redis;

using smartHookahCommon;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = smartHookah.Models.SmokeSession;

    public class SmokeSessionService : ISmokeSessionService
    {
        private readonly SmartHookahContext db;

        public SmokeSessionService(SmartHookahContext db)
        {
            this.db = db;
        }

        #region  Getterss

        public DynamicSmokeStatistic GetRedisData(string id)
        {
            var result = RedisHelper.GetSmokeStatistic(null, id);
            return result;
        }

        public SmokeSessionStatistics GetStatistics(string id)
        {
            var session = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            var result = session?.Statistics;
            return result;
        }

        public SmokeSessionMetaData GetMetaData(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.MetaData).FirstOrDefault(s => s.SessionId == id);
            var result = session?.MetaData;
            return result;
        }

        public HookahSetting GetStandSettings(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.Hookah.Setting)
                .FirstOrDefault(s => s.SessionId == id);
            var result = session?.Hookah.Setting;
            return result;
        }

        public SmokeSession GetLiveSmokeSession(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.MetaData)
                .FirstOrDefault(a => a.SessionId == id);
            if (session != null)
            {
                session.DynamicSmokeStatistic = this.GetRedisData(id);
            }

            return session;
        }

        #endregion
    }
}