using System.Linq;

namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    using smartHookah.Models;
    using smartHookah.Models.Db;

    public class PersonHookahSettingService
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public PersonHookahSettingService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public bool SetDefault(int id)
        {

            var personSetting = this.db.HookahPersonSetting.FirstOrDefault(a => a.Id == id);

            if (personSetting == null) return false;


            var person = this.personService.GetCurentPerson();
            var oldDefault = person.Settings.FirstOrDefault(a => a.Defaut);
            if (oldDefault != null)
            {
                oldDefault.Defaut = false;
                this.db.HookahPersonSetting.AddOrUpdate(oldDefault);
            }
            personSetting.Defaut = true;
            this.db.HookahPersonSetting.AddOrUpdate(personSetting);

            this.db.SaveChanges();

            return true;
        }

        public bool AddSetting(string name, HookahSetting setting)
        {
            var person = this.personService.GetCurentPerson();
            var posibleMatch = this.db.HookahPersonSetting.FirstOrDefault(a => a.Person.Id == person.Id && a.Name == name);
            if (posibleMatch != null)
            {
                posibleMatch.Setting = setting;
                this.db.HookahPersonSetting.AddOrUpdate(posibleMatch);
                this.db.SaveChanges();
                return true;
            }

            var newSetting = new HookahPersonSetting() { Name = name, Person = person, Setting = setting, };
            this.db.HookahPersonSetting.AddOrUpdate(newSetting);
            this.db.SaveChanges();
            return true;
        }

        public ICollection<HookahPersonSetting> GetSettings()
        {
            var person = this.personService.GetCurentPerson();
            return person.Settings.ToList();
        }

    }
}