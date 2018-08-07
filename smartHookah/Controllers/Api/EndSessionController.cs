using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ApplicationInsights;
using smartHookah.Models;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class EndSessionController : ApiController
    {
        private readonly IRedisService _redisService;
        private readonly SmartHookahContext db;
        private TelemetryClient telemetry = new TelemetryClient();
        public EndSessionController(SmartHookahContext db, IRedisService redisService)
        {
            this.db = db;
            _redisService = redisService;
        }

        [HttpPost]
        [ActionName("DefaultAction")]
        public async Task<string> End(string id)
        {
            try
            {
                var reddisSession = _redisService.GetSmokeSessionId(id);

                var stats = _redisService.GetDynamicSmokeStatistic(id);

                if (stats != null && stats.PufCount > 0)
                    await Controllers.SmokeSessionController.EndSmokeSession(reddisSession, db, _redisService);
            }
            catch (Exception e)
            {
                telemetry.TrackException(e);
            }
            return null;

        }
    }
}
