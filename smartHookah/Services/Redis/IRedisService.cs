namespace smartHookahCommon
{
    using System.Collections.Generic;

    using smartHookah.Models.Redis;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        string GetSessionId(string sessionId);

        DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId);

        void StoreAdress(string adress, string name);

        IList<string> GetAdress(string key);
    }
}