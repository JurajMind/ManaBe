using System.Data.Entity;
using System.Linq;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    using smartHookah.Models;
    using smartHookah.Models.Db;

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

            var personSetting = this.db.DevicePreset.FirstOrDefault(a => a.Id == id);

            if (personSetting == null) return false;


            var person = this.personService.GetCurentPerson();
            var oldDefault = person.Settings.FirstOrDefault(a => a.Defaut);
            if (oldDefault != null)
            {
                oldDefault.Defaut = false;
                this.db.DevicePreset.AddOrUpdate(oldDefault);
            }
            personSetting.Defaut = true;
            this.db.DevicePreset.AddOrUpdate(personSetting);

            this.db.SaveChanges();

            return true;
        }

        public bool AddSetting(string name, DeviceSetting setting)
        {
            var person = this.personService.GetCurentPerson();
            var posibleMatch = this.db.DevicePreset.FirstOrDefault(a => a.Person.Id == person.Id && a.Name == name);
            if (posibleMatch != null)
            {
                posibleMatch.Setting = setting;
                this.db.DevicePreset.AddOrUpdate(posibleMatch);
                this.db.SaveChanges();
                return true;
            }

            var newSetting = new DevicePreset() { Name = name, Person = person, Setting = setting, };
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
            var setting = await db.HookahSettings.FirstOrDefaultAsync(a => a.Id == id);
            if (setting == null)
            {
                throw new ItemNotFoundException($"Setting id {id} not found.");
            }
            db.Entry(setting).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
    }
}