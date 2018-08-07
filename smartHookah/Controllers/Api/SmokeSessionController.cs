using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Services.SmokeSession;

using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/SmokeSession")]
    public class SmokeSessionController : ApiController
    {
        private readonly SmartHookahContext db;
        private readonly IRedisService _redisService;

        private readonly ISmokeSessionService sessionService;

        public SmokeSessionController(SmartHookahContext db, ISmokeSessionService sessionService, IRedisService redisService)
        {
            this.db = db;
            this.sessionService = sessionService;
            _redisService = redisService;
        }

        #region Getters and Validators

        [HttpGet]
        [Route("Validate")]
        public ValidationDTO Validate(string id)
        {
            if (id == null || id.Length != 5)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };
            id = id.ToUpper();
            var redisSessionId = _redisService.GetHookahId(id);
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
            
            var smokeSession =  SmokeSessionSimpleDto.FromModel(session);

            var standSetting = StandSettings.FromModel(session.Hookah.Setting);
            return new InitDataDto() { SmokeSession = smokeSession, StandSettings = standSetting };
        }

        #endregion
    }
}