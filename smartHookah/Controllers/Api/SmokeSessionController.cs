﻿using smartHookah.ErrorHandler;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;
using smartHookah.Services.Person;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{


    [RoutePrefix("api/SmokeSession")]
    public class SmokeSessionController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly ISmokeSessionService sessionService;

        private readonly IRedisService redisService;

        private readonly ITobaccoService tobaccoService;

        private readonly IPersonService personService;

        public SmokeSessionController(SmartHookahContext db, ISmokeSessionService sessionService, IRedisService redisService, ITobaccoService tobaccoService, IPersonService personService)
        {
            this.db = db;
            this.sessionService = sessionService;
            this.redisService = redisService;
            this.tobaccoService = tobaccoService;
            this.personService = personService;
        }

        #region Getters and Validators

        [HttpGet]
        [Route("Validate")]
        public ValidationDTO Validate(string id)
        {
            if (id == null || id.Length != 5)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };
            id = id.ToUpper();
            var redisSessionId = this.redisService.GetHookahId(id);
            var dbSession = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == id);

            if (dbSession == null) return new ValidationDTO() { Success = false, Message = "Session not found." };

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

        [HttpGet]
        [Route("GetSessionCode")]
        [ApiAuthorize(Roles = "Admin")]
        public object GetSessionCode(string id)
        {
            return new { sessionCode = this.redisService.GetSessionId(id) };

        }

        [HttpGet]
        [Route("InitData")]
        public InitDataDto InitData(string id)
        {
            if (id == null || id.Length != 5)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Id \'{id}\' not valid."));
            }

            var session = this.sessionService.GetLiveSmokeSession(id);

            if (session == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Id \'{id}\' was not found."));
            }

            var smokeSession = SmokeSessionSimpleDto.FromModel(session);

            var deviceSetting = DeviceSettingDto.FromModel(session.Hookah.Setting);
            return new InitDataDto() { SmokeSession = smokeSession, DeviceSettings = deviceSetting };
        }

        [HttpGet, Route("GetMetaData")]
        public SmokeSessionMetaDataDto GetMetaData(string id)
        {
            try
            {
                var metadata = sessionService.GetSessionMetaData(id);
                return SmokeSessionMetaDataDto.FromModel(metadata);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }

        [HttpGet, Route("GetFinishedData")]
        public FinishedSessionDataDto GetFinishedData(int id)
        {
            var data = this.sessionService.GetSmokeSession(id);
            var personId = this.personService.GetCurentPersonId();
            return new FinishedSessionDataDto()
            {
                Data = SmokeSessionSimpleDto.FromModel(data),
                MetaData = SmokeSessionMetaDataDto.FromModel(data.MetaData),
                Statistics = SmokeSessionStatisticsDto.FromModel(data.Statistics),
                Assigned = data.IsPersonAssign(personId)
            };
        }

        [HttpGet, Route("GetPufs")]
        public async Task<IList<Puf>> GetPufs(int id)
        {
            var pufs = await this.sessionService.GetSmokeSessionPufs(id);

            return pufs.Select(p => new Puf(p)).ToList();
        }

        #endregion

        #region Post methods

        [HttpPost, Route("{id}/SaveMetaData")]
        public async Task<SmokeSessionMetaDataDto> SaveMetaData(string id, SmokeSessionMetaDataDto model)
        {
            try
            {
                if (model.TobaccoMix != null)
                {
                    TobaccoMix newMix = TobaccoMixSimpleDto.ToModel(model.TobaccoMix);
                    var createdMix = await this.tobaccoService.AddOrUpdateMix(newMix);
                    model.TobaccoId = createdMix.Id;
                }

                var result = await this.sessionService.SaveMetaData(id, model.ToModel());
                return SmokeSessionMetaDataDto.FromModel(result);
            }
            catch (Exception e)
            {
                throw new HttpRequestException(e.Message);
            }
        }

        [HttpPost, Route("{id}/End")]
        public async Task<SmokeSessionSimpleDto> EndSmokeSession(string id)
        {
            var endedESession = await this.sessionService.EndSmokeSession(id, SessionReport.Good);
            return SmokeSessionSimpleDto.FromModel(endedESession);
        }

        #endregion
    }
}