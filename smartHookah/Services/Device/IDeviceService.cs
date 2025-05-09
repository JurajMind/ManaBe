﻿using smartHookah.Helpers;
using smartHookah.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace smartHookah.Services.Device
{

    public interface IDeviceService
    {
        Task SetAnimation(string deviceId, Animation animation, PufType state);
        Task SetBrightness(string deviceId, int brightness, PufType state);
        Task SetSpeed(string deviceId, int speed, PufType state);
        Task SetColor(string deviceId, Color color, PufType state);
        Task Sleep(string deviceId);
        Task Restart(string deviceId);
        Task Ping(string id);
        Task SetMode(string deviceId, int mode);
        Task ShowQrCode(string deviceId);
        Task SetPreset(string deviceId, int settingId);
        Task SetPreset(string deviceId, DeviceSetting setting);

        Task UpdateSettingFromDevice(string deviceId, byte[] rawData);

        List<Animation> GetAnimations();
        Animation GetAnimation(int id);
        int GetDeviceVersion(string id);

        DeviceSetting GetStandSettings(string id);

        Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds);

        string GetDeviceInitString(string id, int hookahVersion);

        string GetUpdatePath(string deviceId, string token);

        Task SetPercentage(string code, int pufCount);


        ICollection<Models.Db.SmokeSession> Sessions(int id, int pageSize, int page);

        Task<bool> DeleteDevice(string code);

    }
}