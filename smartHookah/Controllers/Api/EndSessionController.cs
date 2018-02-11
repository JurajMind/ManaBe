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
        private readonly SmartHookahContext db;
        private TelemetryClient telemetry = new TelemetryClient();
        public EndSessionController(SmartHookahContext db)
        {
            this.db = db;
        }

        [HttpPost]
        [ActionName("DefaultAction")]
        public async Task<string> End(string id)
        {
            try
            {
                var reddisSession = RedisHelper.GetSmokeSessionId(id);

                var stats = RedisHelper.GetSmokeStatistic(hookahId: id);

                if (stats != null && stats.PufCount > 0)
                    await SmokeSessionController.EndSmokeSession(reddisSession, db);
            }
            catch (Exception e)
            {
                telemetry.TrackException(e);
            }
            return null;

        }
    }
}
