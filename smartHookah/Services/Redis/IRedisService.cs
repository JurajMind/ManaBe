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

        void StoreAdress(string adress, string name);

        IList<string> GetAdress(string key);

        IList<Puf> GetPufs(string sessionId);

        void SetReservationUsage(int placeId, DateTime date, ReservationUsage reservationUsage);

        ReservationUsage GetReservationUsage(int placeId, DateTime date);
    }
}