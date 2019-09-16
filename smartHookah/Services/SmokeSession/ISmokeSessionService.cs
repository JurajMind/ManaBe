using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Models.Redis;

namespace smartHookah.Services.SmokeSession
{
    using SmokeSession = Models.Db.SmokeSession;

    public interface ISmokeSessionService
    {
        DynamicSmokeStatistic GetDynamicStatistic(string sessionId,string deviceId);

        Dictionary<string, DynamicSmokeStatistic> GetDynamicSmokeStatistics(List<Hookah> hookah,
            Func<Hookah, string> getCode);

        SmokeSessionStatistics GetStatistics(string id);

        Task<ICollection<DbPuf>> GetSmokeSessionPufs(int id);

        SmokeSessionMetaData GetMetaData(int id);
        SmokeSessionMetaData GetSessionMetaData(string id);
        DeviceSetting GetStandSettings(string id);
        SmokeSession GetLiveSmokeSession(string sessionId);

        List<SmokeSession> GetLiveSmokeSessions(List<string> sessions);

        List<SmokeSession> GetDevicesLiveSessions(List<int> deviceId);

        Task<SmokeSessionMetaData> SaveMetaData(string id, SmokeSessionMetaData model);

        Task<SmokeSession> EndSmokeSession(string id, SessionReport source);
        
        void StoreOldPufs();

        SmokeSession GetSmokeSession(int id);

        SmokeSession GetSmokeSession(string sessionId);

        Task UpdateDevicePercentage(int sessionId);
    }
}
