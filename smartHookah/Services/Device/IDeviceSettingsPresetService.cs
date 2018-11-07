using System.Collections.Generic;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Services.Person
{
    public interface IDeviceSettingsPresetService
    {
        bool SetDefault(int id);
        bool AddSetting(string name, DeviceSetting setting);
        ICollection<DevicePreset> GetSettings();
        void Delete(int id);

        bool UseDefaut(string id);
    }
}