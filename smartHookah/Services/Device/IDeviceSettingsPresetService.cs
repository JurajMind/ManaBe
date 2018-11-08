using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Services.Person
{
    public interface IDeviceSettingsPresetService
    {
        bool SetDefault(int presetId);
        bool SetDefault(string sessionCode);
        int AddPreset(string name, DeviceSetting setting);
        int SavePreset(string sessionCode, string name = "", bool addToPerson = true);
        int SavePreset(int deviceId, string name = "", bool addToPerson = true);
        ICollection<DevicePreset> GetUserPresets();
        Task<DevicePreset> GetPreset(int id);
        void Delete(int id);
        bool UseDefaut(string id);
    }
}