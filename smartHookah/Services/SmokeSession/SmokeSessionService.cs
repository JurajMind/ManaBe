using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using smartHookah.Models;
using ServiceStack.Common;
using smartHookah.Models.Redis;
using smartHookahCommon;
using System.Threading.Tasks;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = smartHookah.Models.SmokeSession;
    using smartHookah.Support;

    public class SmokeSessionService : ISmokeSessionService
    {
        private readonly SmartHookahContext _db;
        private readonly IRedisService _redisService;

        public SmokeSessionService(SmartHookahContext db, IRedisService redisService)
        {
            this._db = db;
            _redisService = redisService;
        }

        public DynamicSmokeStatistic GetRedisData(string id)
        {
            var result = _redisService.GetDynamicSmokeStatistic(id);
            return result;
        }

        public SmokeSessionStatistics GetStatistics(string id)
        {
            var session = this._db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            var result = session?.Statistics;
            return result;
        }

        public SmokeSessionMetaData GetMetaData(string id)
        {
            var session = this._db.SmokeSessions.Include(a => a.MetaData).FirstOrDefault(s => s.SessionId == id);
            var result = session?.MetaData;
            return result;
        }

        public HookahSetting GetStandSettings(string id)
        {
            var session = this._db.SmokeSessions.Include(a => a.Hookah).Include(a => a.Hookah.Setting).FirstOrDefault(s => s.SessionId == id);
            var result = session?.Hookah.Setting;
            return result;
        }

        public SmokeSession GetLiveSmokeSession(string id)
        {
            var session = this._db.SmokeSessions.Include(a => a.Hookah).Include(a => a.MetaData).FirstOrDefault(a => a.SessionId == id);
            if (session != null)
            {
                session.DynamicSmokeStatistic = this.GetRedisData(id);
                
            }
            return session;
        }

        public SmokeSession InitSmokeSession(string deviceId = null, string sessionId = null)
        {
            var newSessionId = "";
            newSessionId = string.IsNullOrEmpty(sessionId) ? _redisService.GetSmokeSessionId(deviceId) : sessionId;
            
            var session = _db.SmokeSessions.FirstOrDefault(s => s.SessionId == newSessionId);
            
            if (session != null) return session;

            var dbSession = new SmokeSession
            {
                SessionId = newSessionId,

                MetaData = new SmokeSessionMetaData(),
                Token = Support.RandomString(10)
            };
            var hookah = _db.Hookahs.Where(a => a.Code == deviceId).Include(a => a.Owners).FirstOrDefault();

            if (hookah != null)
            {
                dbSession.Hookah = hookah;

                if (hookah.DefaultMetaData != null)
                    dbSession.MetaData.Copy(hookah.DefaultMetaData);

                foreach (var hookahOwner in hookah.Owners.Where(a => a.AutoAssign))
                {
                    if (hookahOwner.SmokeSessions.Any(a => a.SessionId == dbSession.SessionId))
                        continue;

                    hookahOwner.SmokeSessions.Add(dbSession);
                    _db.Persons.AddOrUpdate(hookahOwner);
                }

                foreach (var placeOwner in hookah.Owners.Where(a => a.Place != null))
                    dbSession.PlaceId = placeOwner.Place.Id;
            }

            _db.SmokeSessions.Add(dbSession);
            _db.SaveChanges();
            return dbSession;
        }

        public bool FindDuplicate(string sessionId)
        {
            return _db.SmokeSessions.Count(a => a.SessionId == sessionId) > 0;
        }

        public string CreateSmokeSession(string hookahId)
        {
            var sessionId = Support.RandomString(5);

            var duplicate = FindDuplicate(sessionId);
            while (duplicate)
            {
                sessionId = Support.RandomString(5);
                duplicate = FindDuplicate(sessionId);
            }

            InitSmokeSession(hookahId, sessionId);

            _redisService.CreateRedisSession(hookahId, sessionId);
            
            return sessionId;
        }
    }
}