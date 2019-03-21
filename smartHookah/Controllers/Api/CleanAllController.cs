using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookah.Helpers;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class CleanAllController : ApiController
    {

        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public void Clean(string id)
        {
            if (id == "mimi")
            {
                RedisHelper.CleanAll();
            }
        }
    }
}
