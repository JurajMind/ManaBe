﻿namespace smartHookahCommon
{
    using smartHookah.Models.Redis;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId);

        void StoreAdress(string adress, string name);
    }
}