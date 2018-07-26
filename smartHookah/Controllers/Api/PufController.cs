using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ApplicationInsights;

using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Support;

namespace smartHookah.Controllers.Api
{
    using smartHookah.Services.SmokeSession;

    public class PufController : ApiController
    {
        
        private readonly TelemetryClient telemetry = new TelemetryClient();

        private readonly ILiveSmokeSessionService liveSmokeSessionService;

        private readonly SmokeSessionHub smokeSessionHub;

        public PufController(ILiveSmokeSessionService liveSmokeSessionService, SmokeSessionHub smokeSessionHub)
        {
            this.liveSmokeSessionService = liveSmokeSessionService;
            this.smokeSessionHub = smokeSessionHub;
        }

        [HttpPost]
        [ActionName("DefaultAction")]
        [OptionalHttps(true)]
        public async Task<string> Put(string id)
        {
            try
            {
                string result = await Request.Content.ReadAsStringAsync();
                if (result.StartsWith("puf"))
                {
                    var puf = this.ParsePuf(result, DateTime.Now);
                    this.liveSmokeSessionService.AddPuf(puf, id);
                }
                return null;
            }
            catch (Exception e)
            {
                telemetry.TrackException(e);
            }
            return null;

        }
        [HttpPost]
        [Route("api/Puf/lag/{id}")]
        public async Task<string> Lag(string id)
        {
           System.Threading.Thread.Sleep(2000);
           return await Put(id);
        }

        private Puf ParsePuf(string data, DateTime enqueuedTime)
        {

            var direction = ToPufType(data);
            var dataChunk = data.Split(':');
            long milis = 0;
            if (dataChunk.Length > 2)
                milis = long.Parse(dataChunk[2]);

            var presure = 0;
            if (dataChunk.Length > 3)
                presure = Convert.ToInt32(float.Parse(dataChunk[3], CultureInfo.InvariantCulture));

            return new Puf(null, direction, enqueuedTime, milis, presure);
            
        }

        private static PufType ToPufType(string data)
        {
            if (data.StartsWith("puf:") && (data.Length >= 5))
            {
                var a = (int)char.GetNumericValue(data[4]);

                return (PufType)a;
            }
            return PufType.Idle;
        }

    }
}