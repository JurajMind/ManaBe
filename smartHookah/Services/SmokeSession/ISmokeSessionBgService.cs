using System.Threading.Tasks;
using smartHookah.Models.Db;
using smartHookah.Models.Redis;

namespace smartHookah.Services.SmokeSession
{
    public interface ISmokeSessionBgService
    {
        DynamicSmokeStatistic GetDynamicStatistic(string sessionId, string deviceId);

        Task<Models.Db.SmokeSession> EndSmokeSession(string id, SessionReport source);

        Task<Models.Db.SmokeSession> InitSmokeSession(string deviceId);

        Task IntNewSessions();
    }
}