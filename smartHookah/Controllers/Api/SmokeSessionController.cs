﻿using System;
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
    public class SmokeSessionController : ApiController
    {
        private readonly SmartHookahContext _db;

        public SmokeSessionController(SmartHookahContext db)
        {
            this._db = db;
        }

        #region Getters and Validators

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("Validate")]
        public async Task<ValidationDTO> Validate(string id)
        {   
            if (id == null || id.Length != 5)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };
            id = id.ToUpper();
            var redisSessionId = RedisHelper.GetHookahId(id);
            var dbSession = _db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);

            if (dbSession == null)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };

            if (redisSessionId == null && dbSession.Statistics != null)
                return new ValidationDTO()
                {
                    Success = true,
                    Message = $"Session with id {id} is not live.",
                    Id = id,
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
