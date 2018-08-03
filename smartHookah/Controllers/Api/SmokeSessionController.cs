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

        public SmokeSessionController(SmartHookahContext db)
        {
            this.db = db;
        }

        #region Getters and Validators

        [HttpGet]
        [Route("Validate")]
        public ValidationDTO Validate(string id)
        {
            if (id == null || id.Length != 5)
                return new ValidationDTO() { Success = false, Message = "Session id is not valid." };
            id = id.ToUpper();
            var redisSessionId = RedisHelper.GetHookahId(id);
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
        public InitDataDTO InitData(string id)
        {
            if (id == null || id.Length != 5)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Id \'{id}\' not valid."));
            }

            var service = new InitDataService(this.db);

            var redis = service.GetRedisData(id);
            var stats = service.GetStatistics(id);
            var metadata = service.GetMetaData(id);
            var settings = service.GetStandSettings(id);

            if (redis == null && stats == null && settings == null && metadata == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"No data found for id \'{id}\'."));
            }

            return new InitDataDTO()
                       {
                           RedisStatistics = redis,
                           SessionMetaData = SmokeSessionMetaDataDto.FromModel(metadata),
                           StandSettings = settings == null ? null : new StandSettings(settings)
                       };
        }

        #endregion
    }
}