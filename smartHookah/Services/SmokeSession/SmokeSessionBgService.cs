using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EntityFramework.BulkInsert;
using EntityFramework.BulkInsert.Extensions;
using smartHookah.Controllers;
using smartHookah.Models.Db;
using smartHookah.Models.Redis;
using smartHookah.Services.Device;
using smartHookah.Services.Redis;

namespace smartHookah.Services.SmokeSession
{
    public class SmokeSessionBgService : ISmokeSessionBgService
    {
        private readonly SmartHookahContext db;
        private readonly IRedisService redisService;
        private readonly IIotService iotService;

        public SmokeSessionBgService(SmartHookahContext db, IRedisService redisService, IIotService iotService)
        {
            this.db = db;
            this.redisService = redisService;
            this.iotService = iotService;
        }

        public DynamicSmokeStatistic GetDynamicStatistic(string sessionId, string deviceId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = this.redisService.GetSessionId(deviceId);
            }
            var result = this.redisService.GetDynamicSmokeStatistic(sessionId);

            if (result == null)
            {
                result = new DynamicSmokeStatistic();
                result.FullUpdate(this.redisService.GetPufs(sessionId).ToList(), sessionId);
                result.LastPufs = new List<Puf>();
                this.redisService.SetDynamicSmokeStatistic(sessionId,result);
            }

            return result;
        }

        public async Task<Models.Db.SmokeSession> EndSmokeSession(string id, SessionReport source)
        {
            // Get hookah code from session Id
            var deviceId = this.redisService.GetHookahId(id);
            var rawPufs = this.redisService.GetPufs(id);

            if (!rawPufs.Any())
                return null;

            var smokeSession = new Models.Db.SmokeSession();

            var dbSmokeSession = db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);
            if (dbSmokeSession != null)
                smokeSession = dbSmokeSession;

            smokeSession.SessionId = id;
            var hookahCode = this.redisService.GetHookahId(id);
            smokeSession.Hookah = db.Hookahs.FirstOrDefault(a => a.Code == hookahCode);
            smokeSession.Statistics = new SmokeSessionStatistics(rawPufs);

            smokeSession.Report = source;

            if (smokeSession.Hookah != null)
            {
                var defaultAnimation = smokeSession.Hookah.Owners.Where(a => a.DefaultSetting != null)
                    .Select(a => a.DefaultSetting).FirstOrDefault();
                if (defaultAnimation != null)
                {
                    smokeSession.Hookah.Setting.Change(defaultAnimation);
                    db.HookahSettings.AddOrUpdate(smokeSession.Hookah.Setting);
                }
            }

            db.SmokeSessions.AddOrUpdate(smokeSession);
            db.SaveChanges();

            var pufs = rawPufs.Select(a => new DbPuf(a) { SmokeSession_Id = smokeSession.Id }).ToList();
            var options = new BulkInsertOptions { BatchSize = 1000 };
            // Default is 5000
            db.BulkInsert(pufs, options);

            db.SaveChanges();
            this.redisService.CleanSmokeSession(id);
            await this.InitSmokeSession(deviceId);
            if (smokeSession.MetaData != null && smokeSession.MetaData.TobaccoId.HasValue)
                TobaccoController.CalculateStatistic(smokeSession.MetaData.TobaccoId.Value, db);

            await this.iotService.SendMsgToDevice(deviceId, "restart:");

            GamificationController.Engine.ManualValidateSession(smokeSession.Id);


            return smokeSession;
        }

        public async Task<Models.Db.SmokeSession> InitSmokeSession(string deviceId)
        {
            var newSessionId = this.CreateSessionId();

            var session = db.SmokeSessions.FirstOrDefault(s => s.SessionId == newSessionId);

            this.redisService.CreateSmokeSession(newSessionId, deviceId);

            if (session != null) return session;

            var dbSession = new Models.Db.SmokeSession
            {
                SessionId = newSessionId,
                MetaData = new SmokeSessionMetaData(),
                Token = smartHookahCommon.Support.Random.RandomString(10),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var hookah = db.Hookahs.Where(a => a.Code == deviceId).Include(a => a.Owners).FirstOrDefault();

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
                    db.Persons.AddOrUpdate(hookahOwner);
                }

                foreach (var placeOwner in hookah.Owners.Where(a => a.Place != null))
                    dbSession.PlaceId = placeOwner.Place.Id;
            }
            try
            {
                db.SmokeSessions.Add(dbSession);

                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return dbSession;
        }

        public async Task IntNewSessions()
        {
            var devices = this.db.Hookahs.Select(s => s.Code).ToList();
            foreach (var device in devices)
            {
                await this.InitSmokeSession(device);
            }
        }

        private string CreateSessionId()
        {
            var sessionId = smartHookahCommon.Support.Random.RandomString(5);

            var duplicate = IsNotUniq(sessionId);
            while (duplicate)
            {
                sessionId = smartHookahCommon.Support.Random.RandomString(5);
                duplicate = IsNotUniq(sessionId);

            }

            return sessionId;
        }

        private bool IsNotUniq(string sessionId)
        {
            return db.SmokeSessions.Count(a => a.SessionId == sessionId) > 0;
        }

    }
}