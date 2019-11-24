using smartHookah.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace smartHookah.Services.Person
{
    public interface IDeviceSettingsPresetService
    {
        bool SetDefault(int presetId);

        bool SetDefault(string sessionCode);

        DevicePreset AddPreset(string name, DeviceSetting setting);

        DevicePreset SaveSessionPreset(string sessionCode, string name = "", bool addToPerson = true);

        DevicePreset SaveDevicePreset(string deviceId, string name = "", bool addToPerson = true);

        ICollection<DevicePreset> GetUserPresets();

        Task<DevicePreset> GetPreset(int id);

        Task<IList<DevicePreset>> GetPresets();

        Task Delete(int id);

        Task<bool> UseDefaut(string id);

        Task<bool> UsePreset(string deviceId, int presetId);

    }
}