namespace smartHookahCommon
{
    using System.Collections.Generic;

    using smartHookah.Models;
    using smartHookah.Models.Redis;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        string GetSmokeSessionId(string hookahId);

        DynamicSmokeStatistic GetSessionStatistic(string sessionId);

        void SetSmokeSessionStatistic(string sessionId, DynamicSmokeStatistic ds);

        DynamicSmokeStatistic GetHookahStatistic(string hookahId);

        void SetHookahStatistic(string hookahId, DynamicSmokeStatistic ds);

        List<Puf> GetPufs(string sessionId);
        void AddPuf(Puf puf, string hookahId);

        bool IsLiveSesssion(string sessionId);
    }
}