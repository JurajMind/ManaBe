using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.TeamFoundation.Client.Reporting;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/SmokeSession")]
    public class SessionController : ApiController
    {
        private readonly SmartHookahContext _db;

        public SessionController(SmartHookahContext db)
        {
            this._db = db;
        }

        #region Getters and Validators

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Validate")]
        public async Task<ValidationDTO> Validate(string smokeSessionId)
        {
            smokeSessionId = smokeSessionId.ToUpper();
            
            if (smokeSessionId.Length != 5)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };

            var redisSessionId = RedisHelper.GetHookahId(smokeSessionId);
            var dbSession = _db.SmokeSessions.FirstOrDefault(a => a.SessionId == smokeSessionId);

            if (dbSession == null)
                return new ValidationDTO() { Success = false, Message = "Session not found." };

            if (redisSessionId == null && dbSession.Statistics != null)
                return new ValidationDTO()
                {
                    Success = true,
                    Message = $"Session with id {smokeSessionId} is not live.",
                    Id = smokeSessionId,
                    Flag = SessionState.Completed
                };

            if (redisSessionId != null && dbSession.Statistics == null)
                return new ValidationDTO()
                {
                    Success = true,
                    Message = $"Session with id {redisSessionId} is live.",
                    Id = redisSessionId,
                    Flag = SessionState.Live
                };

            return new ValidationDTO() { Success = false, Message = "Session not found." };
        }

        #endregion

    }
}
