using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using smartHookah.Models.Db;
using smartHookah.Services.Person;

namespace smartHookah.Services.FeatureMix
{
    internal class FeatureMixService : IFeatureMixService
    {
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        public FeatureMixService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public FeatureMixCreator GetFeatureMixCreator(int id)
        {
            return this.db.FeatureMixCreators.Find(id);
        }

        public IList<FeatureMixCreator> GetFeatureMixCreators()
        {
            return this.db.FeatureMixCreators.ToList();
        }

        public IList<FeatureMixCreator> GetFollowedMixCreators()
        {
            var person = this.personService.GetCurentPerson();
            return person.FollowedMixCreators.ToList();
        }

        public void AddFollow(int creatorId)
        {
            var person = this.personService.GetCurentPerson();
            var curentPerson = this.db.Persons.Find(person.Id);
            var creator = this.GetFeatureMixCreator(creatorId);
            curentPerson.FollowedMixCreators.Add(creator);
            this.db.Persons.AddOrUpdate(curentPerson);
            this.db.SaveChanges();
        }

        public void RemoveFollow(int creatorId)
        {
            var person = this.personService.GetCurentPerson();
            var curentPerson = this.db.Persons.Find(person.Id);
            var creator = this.GetFeatureMixCreator(creatorId);
            curentPerson.FollowedMixCreators.Remove(creator);
            this.db.Persons.AddOrUpdate(curentPerson);
            this.db.SaveChanges();
        }

        public void CreateFeatureMixCreatorFromOld()
        {
            var creator = this.db.Persons.Where(a => a.AssignedBrand != null);

            foreach (var person in creator)
            {
                var newCreator = new FeatureMixCreator
                {
                    Person = person,
                    Name = person.AssignedBrandId
                };
                this.db.FeatureMixCreators.Add(newCreator);
            }

            this.db.SaveChanges();
        }
    }
}