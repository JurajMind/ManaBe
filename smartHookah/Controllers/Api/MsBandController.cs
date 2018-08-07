using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using smartHookah.Models;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class MsBandController : ApiController
    {
        private readonly IRedisService _redisService;

        public MsBandController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [System.Web.Mvc.ActionName("DefaultAction")]
        public BandData GetData(string id)
        {
            try
            {
                var sessionId = _redisService.GetSmokeSessionId(id);
                var Pufs = _redisService.GetPufs(sessionId);

                var pufCount = Pufs.Count(a => a.Type == PufType.In);
                var sessionStart = Pufs.OrderBy(a => a.DateTime).First().DateTime;
                var smokeDuration = Pufs.GetDuration(puf => puf.Type == PufType.In);

                var onePuf = smokeDuration.Aggregate((a, b) => a + b).Milliseconds / pufCount;
                var posibleSmokeTime = 230 * onePuf;
                var posibleEnd = sessionStart.AddMilliseconds(posibleSmokeTime);

                return new BandData()
                {
                    PufCount = pufCount,
                    SessionEnd = posibleEnd,
                    SessionStart = sessionStart
                };
            }
            catch (Exception)
            {
                
                return new BandData()
                {
                    PufCount =  0,
                    SessionStart =  DateTime.Now,
                    SessionEnd =  DateTime.Now.AddMinutes(90)

                };
            }

        }


        public class BandData
        {
            public int PufCount { get; set; }

            public DateTime SessionStart { get; set; }

            public DateTime SessionEnd { get; set; }
        }
    }
}
