using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using smartHookah.Models;
using smartHookah.Models.Redis;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Controllers.Api
{
    public class WearablesController : ApiController
    {
        [System.Web.Mvc.ActionName("DefaultAction")]
        public DynamicSmokeStatistic GetData(string id)
        {
            try
            {
                return RedisHelper.GetSmokeStatistic(id);
            }
            catch (Exception e)
            {
                return new DynamicSmokeStatistic() {AlertBlowCount = -1};


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
