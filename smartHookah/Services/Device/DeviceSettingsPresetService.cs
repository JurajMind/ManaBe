﻿using System;
using System.Threading.Tasks;

namespace smartHookah.Services.Device
{
    using smartHookah.Models.Db;
    using smartHookah.Services.Person;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

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
            var setting = this.deviceService.GetStandSettings(sessionCode);
            if (setting == null) return false;
            if (setting.DevicePreset != null)
            {
                this.SetDefault(setting.DevicePreset.Id);
                return true;
            }

            var preset = this.AddPreset($"{sessionCode} : {DateTime.UtcNow}", setting);
            return preset.Id > 0;
        }

        public DevicePreset AddPreset(string name, DeviceSetting setting)
        {
            if (setting == null) return null;
            var person = this.personService.GetCurentPerson();
            var posibleMatch = this.db.DevicePreset.FirstOrDefault(a => a.Person.Id == person.Id && a.Name == name);
            if (posibleMatch != null)
            {
                var matchId = posibleMatch.DeviceSetting.Id;

                // Preset edit
                posibleMatch.DeviceSetting = setting;
                posibleMatch.DeviceSetting.Id = matchId;
                this.db.DevicePreset.AddOrUpdate(posibleMatch);
                this.db.SaveChanges();
                return posibleMatch;
            }

            var newSetting = new DeviceSetting(setting);
            var newDevicePreset = new DevicePreset() { Name = name, PersonId = person.Id, DeviceSetting = newSetting };
            this.db.DevicePreset.Add(newDevicePreset);
            this.db.SaveChanges();
            return newDevicePreset;
        }

        public DevicePreset SaveSessionPreset(string sessionCode, string name = "", bool addToPerson = true)
        {
            var session = this.db.SmokeSessions
                .Include(a => a.Hookah.Setting)
                .FirstOrDefault(a => a.SessionId == sessionCode);
            if (session?.Hookah?.Setting == null) return null;
            if (addToPerson) return this.AddPreset(this.CreateName(name, session.Hookah), session.Hookah.Setting);
            var preset = new DevicePreset()
            {
                DeviceSetting = session.Hookah.Setting,
                Name = this.CreateName(name, session.Hookah)
            };
            this.db.DevicePreset.AddOrUpdate(preset);
            this.db.SaveChanges();
            return preset;
        }

        public DevicePreset SaveDevicePreset(string deviceId, string name = "", bool addToPerson = true)
        {
            var device = this.db.Hookahs.Include(a => a.Setting).FirstOrDefault(a => a.Code == deviceId);
            if (device?.Setting == null) return null;
            if (addToPerson) return this.AddPreset(this.CreateName(name, device), device.Setting);

            var preset = new DevicePreset()
            {
                DeviceSetting = new DeviceSetting(device.Setting),
                Name = this.CreateName(name, device)
            };

            this.db.DevicePreset.AddOrUpdate(preset);
            this.db.SaveChanges();
            return preset;
        }

        public ICollection<DevicePreset> GetUserPresets()
        {
            var person = this.personService.GetCurentPerson();
            var personId = person?.Id;
            var presets = this.db.DevicePreset.Include(a => a.DeviceSetting).Where(a => a.PersonId == personId || a.Person == null);
            return presets.ToList();
        }

        public async Task<DevicePreset> GetPreset(int id)
        {
            return await this.db.DevicePreset.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task Delete(int id)
        {
            var setting = await this.db.HookahSettings.FirstOrDefaultAsync(a => a.Id == id);
            if (setting == null)
            {
                throw new KeyNotFoundException($"Setting id {id} not found.");
            }

            this.db.Entry(setting).State = EntityState.Deleted;
            await this.db.SaveChangesAsync();
        }

        public async Task<IList<DevicePreset>> GetPresets()
        {

            var person = this.personService.GetCurentPerson();
            var personId = person?.Id;

            var presets = this.db.DevicePreset.Include(a => a.DeviceSetting)
                .Where(a => a.PersonId == personId || a.PersonId == null);

            return await presets.ToListAsync();
        }

        public async Task<bool> UseDefaut(string id)
        {
            var session = this.db.SmokeSessions.FirstOrDefault(s => s.SessionId == id);
            if (session == null) return false;

            var person = this.personService.GetCurentPerson();

            if (person.DefaultSetting == null) return false;
            return await this.UsePreset(id, person.DefaultPreset.Id);
        }

        public async Task<bool> UsePreset(string deviceId, int presetId)
        {
            var preset = await this.db.DevicePreset.FindAsync(presetId);
            if (preset != null)
            {
                await this.deviceService.SetPreset(deviceId, preset.DeviceSetting);
                return true;
            }

            return false;
        }

        private string CreateName(string name, Hookah device) =>
            string.IsNullOrEmpty(name) ? device == null ? $"Untitled - {DateTime.UtcNow}" : $"{device.Name} - {DateTime.UtcNow}" : name;
    }
}