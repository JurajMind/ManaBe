using smartHookah.Controllers;
using smartHookah.Models.Db;

namespace smartHookah.Services.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using smartHookah.Models;
    using smartHookah.Models.Redis;
    using smartHookah.Services.Place;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        string GetSessionId(string hookahId);

        DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId);

        void SetDynamicSmokeStatistic(string sessionId,DynamicSmokeStatistic dynamicSmokeStatistic);

        void StoreAdress(string adress, string name);

        IList<string> GetAdress(string key);

        IList<Puf> GetPufs(string sessionId);

        void SetReservationUsage(int placeId, DateTime date, ReservationUsage reservationUsage);

        ReservationUsage GetReservationUsage(int placeId, DateTime date);

        bool CleanSmokeSession(string smokeSessionId);

        bool CreateSmokeSession(string smokeSessionId,string deviceId);

        DateTime? GetConnectionTime(string deviceCode);

        void SetConnectionTime(string deviceCode);

        Puf AddPuf(string smokeSessionId, string hookahId, PufType direction, DateTime pufTime, long milis,
            int presure = 0);

        void StoreUpdate(string token,UpdateController.UpdateRedis update);

        UpdateController.UpdateRedis GetUpdate(string token);

        IList<string> GetBrands(string prefix);

        void StoreBrands(IList<string> brands);
    }
}