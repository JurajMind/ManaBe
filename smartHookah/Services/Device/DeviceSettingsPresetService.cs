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

        private readonly IPersonService personService;

        public DeviceSettingsPresetService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public bool SetDefault(int id)
        {
            var defaultPreset = this.db.DevicePreset.FirstOrDefault(a => a.Id == id);

            if (defaultPreset == null) return false;

            var person = this.personService.GetCurentPerson();
            person.DefaultPreset = defaultPreset;
         
            this.db.Persons.AddOrUpdate(person);

            this.db.SaveChanges();

            return true;
        }

        public bool AddSetting(string name, DeviceSetting setting)
        {
            var person = this.personService.GetCurentPerson();
            var posibleMatch = this.db.DevicePreset.FirstOrDefault(a => a.Person.Id == person.Id && a.Name == name);
            if (posibleMatch != null)
            {
                posibleMatch.DeviceSetting = setting;
                this.db.DevicePreset.AddOrUpdate(posibleMatch);
                this.db.SaveChanges();
                return true;
            }

            var newSetting = new DevicePreset() { Name = name, Person = person, DeviceSetting = setting, };
            this.db.DevicePreset.AddOrUpdate(newSetting);
            this.db.SaveChanges();
            return true;
        }

        public ICollection<DevicePreset> GetSettings()
        {
            var person = this.personService.GetCurentPerson();
            return person.Settings.ToList();
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
    }
}