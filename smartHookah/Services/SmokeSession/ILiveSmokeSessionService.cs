namespace smartHookah.Services.SmokeSession
{
    using System.Threading.Tasks;

    using smartHookah.Models;
    using smartHookah.Models.Redis;

    public interface ILiveSmokeSessionService
    {
        void UpdateLiveStatistic(string sessionId, Puf puf);

        bool AddPuf(Puf puf,string hookahId);

        DynamicSmokeStatistic GetSmokeSessionStatistic(string id);
    }
}