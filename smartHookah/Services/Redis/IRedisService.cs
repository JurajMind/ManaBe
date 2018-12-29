namespace smartHookah.Services.Redis
{
    using System.Collections.Generic;

    using smartHookah.Models;
    using smartHookah.Models.Redis;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        string GetSessionId(string hookahId);

        DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId);

        void StoreAdress(string adress, string name);

        IList<string> GetAdress(string key);

        IList<Puf> GetPufs(string sessionId);
    }
}