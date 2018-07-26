using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Services.SmokeSession
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Threading.Tasks;

    using log4net;

    using smartHookah.Models;
    using smartHookah.Support;

    public class SmokeSessionService : ISmokeSessionService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SmokeSessionService));

        private readonly SmartHookahContext db;

        public SmokeSessionService(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<string> CreateSmokeSession(string hookahId)
        {
            var sessionId = this.FindUniqueSession();

            var session = new SmokeSession
                              {
                                  SessionId = sessionId,
                                  MetaData = new SmokeSessionMetaData(),
                                  Token = Support.RandomString(10)
                              };
            var hookah = this.db.Hookahs.Where(a => a.Code == hookahId).Include(a => a.Owners)
                .Include(a => a.DefaultMetaData).FirstOrDefault();

            if (hookah != null)
            {
                session.Hookah = hookah;

                if (hookah.DefaultMetaData != null) session.MetaData.Copy(hookah.DefaultMetaData);

                foreach (var hookahOwner in hookah.Owners.Where(a => a.AutoAssign))
                {
                    hookahOwner.SmokeSessions.Add(session);
                    this.db.Persons.AddOrUpdate(hookahOwner);
                }

                foreach (var placeOwner in hookah.Owners.Where(a => a.Place != null))
                    session.PlaceId = placeOwner.Place.Id;
            }

            this.db.SmokeSessions.Add(session);
            await this.db.SaveChangesAsync();
            return sessionId;
        }

        private bool FindDuplicate(string sessionId)
        {
            return this.db.SmokeSessions.Count(a => a.SessionId == sessionId) > 0;
        }

        private string FindUniqueSession()
        {
            var sessionId = Support.RandomString(5);
            var duplicate = this.FindDuplicate(sessionId);
            while (duplicate)
            {
                sessionId = Support.RandomString(5);
                duplicate = this.FindDuplicate(sessionId);
            }

            return sessionId;
        }
    }

    public interface ISmokeSessionService
    {
        Task<string> CreateSmokeSession(string hookahId);
    }
}