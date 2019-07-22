using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using smartHookah.Models.Redis;
using System.Threading.Tasks;
using smartHookah.Helpers.ModelExtensions;
using smartHookah.Models.Db;
using smartHookah.Services.Device;
using smartHookah.Services.Redis;
using smartHookahCommon.Exceptions;
using smartHookahCommon.Errors;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = Models.Db.SmokeSession;

    public class SmokeSessionService : ISmokeSessionService
    {
        private readonly SmartHookahContext db;
        private readonly IRedisService redisService;
        private readonly IDeviceService deviceService;
        private readonly ISmokeSessionBgService smokeSessionBgService;

        public SmokeSessionService(SmartHookahContext db, IRedisService redisService, IDeviceService deviceService, ISmokeSessionBgService smokeSessionBgService)
        {
            this.db = db;
            this.redisService = redisService;
            this.deviceService = deviceService;
            this.smokeSessionBgService = smokeSessionBgService;
        }

        public DynamicSmokeStatistic GetDynamicStatistic(string sessionId,string deviceId)
        {
           return this.smokeSessionBgService.GetDynamicStatistic(sessionId, deviceId);
        }

        public Dictionary<string, DynamicSmokeStatistic> GetDynamicSmokeStatistics(List<Hookah> hookah, Func<Hookah, string> getCode)
        {
            var result = new Dictionary<string, DynamicSmokeStatistic>();
            foreach (var hookah1 in hookah)
            {
                var ds = this.GetDynamicStatistic(null, hookah1.Code);

                if (ds != null)
                    result.Add(getCode(hookah1), ds);
            }
            return result;
        }

        public SmokeSessionStatistics GetStatistics(string id)
        {
            var session = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            var result = session?.Statistics;
            return result;
        }

        public SmokeSessionMetaData GetMetaData(int id) => db.SessionMetaDatas
            .Include(a => a.Bowl)
            .Include(a => a.Coal)
            .Include(a => a.HeatManagement)
            .Include(a => a.Pipe)
            .Include(a => a.Tobacco)
            .FirstOrDefault(a => a.Id == id);

        public SmokeSessionMetaData GetSessionMetaData(string id)
        {
            var session = db.SmokeSessions
                .Include(a => a.MetaData)
                .FirstOrDefault(a => a.SessionId == id);
            if (session?.MetaData == null)
            {
                throw new KeyNotFoundException($"Session id {id} not found or it has no metadata.");
            }

            return session.MetaData;
        }

        public SmokeSession GetSmokeSession(int id)
        {
            var session = db.SmokeSessions
                .Include(a => a.MetaData).Include(a => a.Statistics)
                .FirstOrDefault(a => a.Id == id);
            if (session == null)
            {
                throw new ManaException(ErrorCodes.SessionNotFound,$"Session id {id} not found or it has no metadata.");
            }

            return session;
        }


        public DeviceSetting GetStandSettings(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.Hookah.Setting).FirstOrDefault(s => s.SessionId == id);
            var result = session?.Hookah.Setting;
            return result;
        }

        public SmokeSession GetLiveSmokeSession(string sessionId)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.MetaData).FirstOrDefault(a => a.SessionId == sessionId);
            if (session != null)
            {
                session.DynamicSmokeStatistic = this.GetDynamicStatistic(sessionId,null);

            }
            return session;
        }

        public async Task<SmokeSessionMetaData> SaveMetaData(string id, SmokeSessionMetaData model)
        {
            if (model == null || string.IsNullOrEmpty(id)) throw new ArgumentNullException();

            var session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            if (session == null) throw new KeyNotFoundException($"Session with id {id} not found.");
            
            if (session.MetaDataId != model.Id)
            {
                model.Id = session.MetaDataId ?? 0;
            }

            db.SessionMetaDatas.AddOrUpdate(model);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return this.db.SessionMetaDatas.Where(a => a.Id == model.Id).Include(a => a.Tobacco).FirstOrDefault();
        }

        public Task<SmokeSession> EndSmokeSession(string id, SessionReport source)
        {
            return this.smokeSessionBgService.EndSmokeSession(id, source);
        }

        public void StoreOldPufs()
        {
            var batchId = smartHookah.Support.Support.RandomString(5);
            var session = this.db.SmokeSessions.Where(s => s.StatisticsId != null && s.StorePath == null).Include(s => s.Statistics).OrderBy(s => s.Statistics.Start)
                .Take(100).ToList();

            foreach (var smokeSession in session)
            {
                var storePath = SmokeSessionPufExtension.StoredPufs(smokeSession, batchId);
                smokeSession.StorePath = storePath;
            
                this.db.Database.ExecuteSqlCommand("DELETE FROM DbPuf where id=@p0",smokeSession.Id);
                this.db.Database.ExecuteSqlCommand("update SmokeSession set StorePath = @p0 where id = @p1", storePath, smokeSession.Id);
       
            }
        }

        public async Task<ICollection<DbPuf>> GetSmokeSessionPufs(int id)
        {
            var session = await this.db.SmokeSessions.FindAsync(id);

            if(session == null)
            {
                throw new ManaException(ErrorCodes.SessionNotFound, $"Session with id {id} not found");
            }

            return session.Pufs;
        }
    }
}