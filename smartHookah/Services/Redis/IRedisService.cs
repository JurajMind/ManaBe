using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using smartHookah.Controllers;
using smartHookah.Models;
using smartHookah.Support;

namespace smartHookahCommon
{
    using smartHookah.Models.Redis;

    public interface IRedisService
    {
        string GetHookahId(string sessionId);

        DynamicSmokeStatistic GetDynamicSmokeStatistic(string sessionId);

        IEnumerable<string> GetSmokeSessionIds(List<string> hookahIds);

        string GetSmokeSessionId(string hookahId);

        List<Puf> GetPufs(string sessionId);

        void CleanAll();

        Puf AddPuff(string smokeSessionId, PufType direction, DateTime pufTime);

        Puf AddPuff(string smokeSessionId, string hookahId, PufType direction, DateTime pufTime, long milis,
            int presure = 0);

        void CreateRedisSession(string hookahId, string sessionId);

        void AddSeat(string sessionId, string seat);

        string GetSeat(string sessionId);

        int GetReservationFromTable(string table);

        void SetReservationToTable(string table, int reservation);

        DateTime? GetConnectionTime(string hookahCode);

        void SetConnectionTime(string hookahCode);

        string GetHookahSeat(string hookahCode);

        Dictionary<string, string> GetHookahSeat(List<string> hookahCode);

        bool EndSmokeSession(string smokeSessionId);

        string GetUpdateFilePath(string id, string token);

        void Update(UpdateController.UpdateRedis updateRedis, string updateToken);

        void UpdateStatistics(string deviceId, Puf puff);

        void OnConnect(string deviceId);
    }
}