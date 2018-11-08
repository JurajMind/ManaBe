using System;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.TeamFoundation.VersionControl.Client;

    using smartHookah.Models;
    using smartHookah.Models.Db;
    using smartHookah.Services.Person;

    public class DeviceSettingsPresetService : IDeviceSettingsPresetService
    {
        private readonly SmartHookahContext db;
        private readonly IDeviceService deviceService;

        private readonly IPersonService personService;

        public DeviceSettingsPresetService(SmartHookahContext db, IPersonService personService, IDeviceService deviceService)
        {
            this.db = db;
            this.personService = personService;
            this.deviceService = deviceService;
        }

        public bool SetDefault(int presetId)
        {
            var defaultPreset = this.db.DevicePreset.FirstOrDefault(a => a.Id == presetId);

            if (defaultPreset == null) return false;

            var person = this.personService.GetCurentPerson();
            person.DefaultPreset = defaultPreset;
         
            this.db.Persons.AddOrUpdate(person);

            this.db.SaveChanges();

            return true;
        }

        public bool SetDefault(string sessionCode)
        {
            var setting = deviceService.GetStandSettings(sessionCode);
            if (setting == null) return false;
            if (setting.DevicePreset != null)
            {
                SetDefault(setting.DevicePreset.Id);
                return true;
            }
            var presetId = AddPreset($"{sessionCode} : {DateTime.UtcNow}", setting);
            return presetId > 0;
        }

        public int AddPreset(string name, DeviceSetting setting)
        {
            if (setting == null) return -1;
            var person = this.personService.GetCurentPerson();
            var posibleMatch = this.db.DevicePreset.FirstOrDefault(a => a.Person.Id == person.Id && a.Name == name);
            if (posibleMatch != null)
            {
                posibleMatch.DeviceSetting = setting;
                this.db.DevicePreset.AddOrUpdate(posibleMatch);
                this.db.SaveChanges();
                return setting.Id;
            }

            var newSetting = new DevicePreset() { Name = name, Person = person, DeviceSetting = setting, };
            this.db.DevicePreset.AddOrUpdate(newSetting);
            this.db.SaveChanges();
            return newSetting.Id;
        }

        public int SavePreset(string sessionCode, string name = "", bool addToPerson = true)
        {
            var session = db.SmokeSessions
                .Include(a => a.Hookah.Setting)
                .FirstOrDefault(a => a.SessionId == sessionCode);
            if (session?.Hookah?.Setting == null) return -1;
            if (addToPerson) return AddPreset(CreateName(name, session.Hookah), session.Hookah.Setting);
            var preset = new DevicePreset()
            {
                DeviceSetting = session.Hookah.Setting,
                Name = CreateName(name, session.Hookah)
            };
            db.DevicePreset.AddOrUpdate(preset);
            db.SaveChanges();
            return preset.Id;
        }

        public int SavePreset(int deviceId, string name = "", bool addToPerson = true)
        {
            var device = db.Hookahs.Include(a => a.Setting).FirstOrDefault(a => a.Id == deviceId);
            if (device?.Setting == null) return -1;
            if (addToPerson) return AddPreset(CreateName(name, device), device.Setting);

            var preset = new DevicePreset()
            {
                DeviceSetting = device.Setting,
                Name = CreateName(name, device)
            };

            db.DevicePreset.AddOrUpdate(preset);
            db.SaveChanges();
            return preset.Id;
        }

        public ICollection<DevicePreset> GetUserPresets()
        {
            var person = this.personService.GetCurentPerson();
            return person.Presets.ToList();
        }

        public async Task<DevicePreset> GetPreset(int id)
        {
            return await db.DevicePreset.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async void Delete(int id)
        {
            var setting = await this.db.HookahSettings.FirstOrDefaultAsync(a => a.Id == id);
            if (setting == null)
            {
                throw new ItemNotFoundException($"Setting id {id} not found.");
            }
            this.db.Entry(setting).State = EntityState.Deleted;
            await this.db.SaveChangesAsync();
        }

        public bool UseDefaut(string id)
        {
            var session = this.db.SmokeSessions.FirstOrDefault(s => s.SessionId == id);
            if (session == null) return false;

            var person = this.personService.GetCurentPerson();

            if (person.DefaultSetting == null) return false;

            return true;

            //@TODO

        }

        private string CreateName(string name, Hookah device) =>
            string.IsNullOrEmpty(name) ? device == null ? $"Untitled - {DateTime.UtcNow}" : $"{device.Name} - {DateTime.UtcNow}" : name;
    }
}