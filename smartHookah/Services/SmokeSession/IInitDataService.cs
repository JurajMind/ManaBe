using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Redis;

namespace smartHookah.Services.SmokeSession
{
    interface IInitDataService
    {
        DynamicSmokeStatistic GetRedisData(string id);
        SmokeSessionStatistics GetStatistics(string id);
        SmokeSessionMetaData GetMetaData(string id);
        HookahSetting GetStandSettings(string id);
    }
}
