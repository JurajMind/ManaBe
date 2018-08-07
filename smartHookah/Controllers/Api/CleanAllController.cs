using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class CleanAllController : ApiController
    {
        private readonly IRedisService _redisService;

        public CleanAllController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public void Clean(string id)
        {
            if (id == "mimi")
            {
                _redisService.CleanAll();
            }
        }
    }
}
