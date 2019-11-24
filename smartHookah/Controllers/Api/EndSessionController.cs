using Microsoft.ApplicationInsights;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    public class EndSessionController : ApiController
    {
        private readonly SmartHookahContext db;
        private readonly ISmokeSessionService sessionService;
        private readonly IRedisService redisService;
        private TelemetryClient telemetry = new TelemetryClient();
        public EndSessionController(SmartHookahContext db, ISmokeSessionService sessionService, IRedisService redisService)
        {
            this.db = db;
            this.sessionService = sessionService;
            this.redisService = redisService;
        }

        [HttpPost]
        [ActionName("DefaultAction")]
        public async Task<string> End(string id)
        {
            try
            {
                var sessionId = this.redisService.GetSessionId(id);
                await this.sessionService.EndSmokeSession(sessionId, SessionReport.FromDevice);
            }
            catch (Exception e)
            {
                telemetry.TrackException(e);
            }
            return null;

        }
    }
}
