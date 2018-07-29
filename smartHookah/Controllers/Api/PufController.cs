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
        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        private TelemetryClient telemetry = new TelemetryClient();

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
                    var puf = SendPuf(id, result, DateTime.Now);
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
            UpdateStistics(deviceId, puf);
        }

        private void UpdateStistics(string deviceId, Puf puf)
        {
            using (var redis = RedisHelper.redisManager.GetClient())
            {
                var session = puf.SmokeSessionId;
                var ds = redis.As<DynamicSmokeStatistic>()["DS:" + session];

                if ((ds == null) || (ds.LastFullUpdate < DateTime.Now.AddMinutes(-5)))
                {
                    if (ds == null)
                        ds = new DynamicSmokeStatistic();
                    ds.FullUpdate(redis, session);
                }
                else
                {
                    if (puf != null)
                        ds.Update(puf, session, deviceId);
                }

                redis.As<DynamicSmokeStatistic>()["DS:" + session] = ds;

                var oldDs = new
                {
                    pufCount = ds.PufCount,
                    lastPuf = ds.LastPufDuration.ToString(@"s\.fff"),
                    lastPufTime = ds.LastPufTime.AddHours(-1).ToString("dd-MM-yyyy HH:mm:ss"),
                    smokeDuration = ds.TotalSmokeTime.ToString(@"hh\:mm\:ss"),
                    longestPuf = ds.LongestPuf.ToString(@"s\.fff"),
                    start = ds.Start.ToString("dd-MM-yyyy HH:mm:ss"),
                    duration = ((DateTime.UtcNow - ds.Start).ToString(@"hh\:mm\:ss")),
                    longestPufMilis = ds.LongestPuf.TotalMilliseconds
                };

                var ownDs = new DynamicSmokeStatisticDto(ds);

                ClientContext.Clients.Group(session).updateStats(oldDs);
                ClientContext.Clients.Group(deviceId).updateStats(deviceId, ownDs);
            }
        }

        private static Puf SendPuf(string connectionDeviceId, string data, DateTime enqueuedTime)
        {

            var direction = ToPufType(data);
            var dataChunk = data.Split(':');
            long milis = 0;
            if (dataChunk.Length > 2)
                milis = long.Parse(dataChunk[2]);

            var presure = 0;
            if (dataChunk.Length > 3)
                presure = Convert.ToInt32(float.Parse(dataChunk[3], CultureInfo.InvariantCulture));

            return RedisHelper.AddPuff(null, connectionDeviceId, direction, enqueuedTime, milis, presure);
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