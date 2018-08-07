using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.ApplicationInsights;
using Microsoft.AspNet.SignalR;
using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class PufController : ApiController
    {
        private readonly IRedisService _redisService;
        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        private TelemetryClient telemetry = new TelemetryClient();

        public PufController(IRedisService redisService)
        {
            _redisService = redisService;
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
                    var puf = SendPuf(id, result, DateTime.Now, _redisService);
                    OnPuf(id, puf);
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

        public void OnPuf(string deviceId, Puf puf)
        {
            var pufType = puf.Type;
            ClientContext.Clients.Group(puf.SmokeSessionId).pufChange(pufType.ToWebStateString(), pufType.ToGraphData());
            UpdateStatistics(deviceId, puf);
        }

        private void UpdateStatistics(string deviceId, Puf puf)
        {
            _redisService.UpdateStatistics(deviceId, puf);
        }

        private static Puf SendPuf(string connectionDeviceId, string data, DateTime enqueuedTime, IRedisService redisService)
        {
            var direction = ToPufType(data);
            var dataChunk = data.Split(':');
            long milis = 0;
            if (dataChunk.Length > 2)
                milis = long.Parse(dataChunk[2]);

            var presure = 0;
            if (dataChunk.Length > 3)
                presure = Convert.ToInt32(float.Parse(dataChunk[3], CultureInfo.InvariantCulture));

            return redisService.AddPuff(null, connectionDeviceId, direction, enqueuedTime, milis, presure);
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