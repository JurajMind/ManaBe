using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Redis;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = smartHookah.Models.SmokeSession;

    public interface ISmokeSessionService
    {
        DynamicSmokeStatistic GetRedisData(string id);
        SmokeSessionStatistics GetStatistics(string id);
        SmokeSessionMetaData GetMetaData(int id);
        HookahSetting GetStandSettings(string id);
        SmokeSession GetLiveSmokeSession(string id);
        Task<SmokeSessionMetaData> SaveMetaData(string id, SmokeSessionMetaData model);
    }
}
