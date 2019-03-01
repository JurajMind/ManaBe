using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web;
using smartHookah.Models;
using ServiceStack.Common;
using smartHookah.Models.Redis;
using smartHookahCommon;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Helpers.ModelExtensions;
using smartHookah.Services.Gear;
using ServiceStack.Common.Utils;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = smartHookah.Models.SmokeSession;

    public class SmokeSessionService : ISmokeSessionService
    {
        private readonly SmartHookahContext db;
        private readonly IGearService gearService;

        public SmokeSessionService(SmartHookahContext db, IGearService gearService)
        {
            this.db = db;
            this.gearService = gearService;
        }

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
                throw new ItemNotFoundException($"Session id {id} not found or it has no metadata.");
            }

            return session.MetaData;
        }


        public DeviceSetting GetStandSettings(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.Hookah.Setting).FirstOrDefault(s => s.SessionId == id);
            var result = session?.Hookah.Setting;
            return result;
        }

        public SmokeSession GetLiveSmokeSession(string id)
        {
            var session = this.db.SmokeSessions.Include(a => a.Hookah).Include(a => a.MetaData).FirstOrDefault(a => a.SessionId == id);
            if (session != null)
            {
                session.DynamicSmokeStatistic = this.GetRedisData(id);
                
            }
            return session;
        }

        public async Task<SmokeSessionMetaData> SaveMetaData(string id, SmokeSessionMetaData model)
        {
            if (model == null || string.IsNullOrEmpty(id)) throw new ArgumentNullException();

            var session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            if (session == null) throw new ItemNotFoundException($"Session with id {id} not found.");
            
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

        public void StoreOldPufs()
        {
            var batchId = smartHookah.Support.Support.RandomString(5);
            var session = this.db.SmokeSessions.Where(s => s.StatisticsId != null && s.StorePath == null).OrderBy(s => s.Statistics.Start)
                .Take(100).ToList();

            foreach (var smokeSession in session)
            {
                var storePath = SmokeSessionPufExtension.StoredPufs(smokeSession, batchId);
                smokeSession.StorePath = storePath;
                this.db.DbPufs.RemoveRange(smokeSession.DbPufs);
                this.db.SmokeSessions.AddOrUpdate(smokeSession);
                db.SaveChanges();
            }

        }
    }
}