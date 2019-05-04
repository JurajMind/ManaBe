using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using smartHookah.Models.Db;
using smartHookah.Services.Person;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

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

        public IList<FeatureMixCreator> GetFeatureMixCreators(int page = 0, int pageSize = 50, string orderBy = "name", string order = "asc")
        {
            var query = from b in this.db.FeatureMixCreators
                select b;

            switch (orderBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? from a in query orderby a.Name ascending select a : from a in query orderby a.Name descending select a;
                    break;
                case "count":
                    query = order.ToLower() == "asc" ? from a in query orderby a.Person.Likes.Count(x => x is TobaccoMix) ascending select a : from a in query orderby a.Person.Likes.Count(x => x is TobaccoMix) descending select a;
                    break;
                default:
                    throw new ManaException(ErrorCodes.WrongOrderField, "Invalid OrderBy value, select \"name\" or \"count\".");
            }

            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            return query.ToList();
        }

        public IList<PipeAccesory> GetCreatorMixes(int creatorId, int page = 0, int pageSize = 50, string orderBy = "name", string order = "asc")
        {
            var query = this.db.FeatureMixCreators.Find(creatorId)
                ?.Person.Likes
                .SelectMany(a => a.Person.Likes.Where(b => b is TobaccoMix).Select(c => c.PipeAccesory));

            switch (orderBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? from a in query orderby a.AccName ascending select a : from a in query orderby a.AccName descending select a;
                    break;
                case "rating":
                    query = order.ToLower() == "asc" ? from a in query orderby a.LikeCount ascending select a : from a in query orderby a.LikeCount descending select a;
                    break;
                default:
                    throw new ManaException(ErrorCodes.WrongOrderField, "Invalid OrderBy value, select \"name\" or \"count\".");
            }

            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            return query.ToList();
        }

        public IList<FeatureMixCreator> GetFollowedMixCreators()
        {
            var person = this.personService.GetCurentPerson();
            return person.FollowedMixCreators.ToList();
        }

        public void AddFollow(int creatorId)
        {
            var person = this.personService.GetCurentPerson();
            var currentPerson = this.db.Persons.Find(person.Id);
            var creator = this.GetFeatureMixCreator(creatorId);
            if (currentPerson != null)
            {
                currentPerson.FollowedMixCreators.Add(creator);
                this.db.Persons.AddOrUpdate(currentPerson);
            }

            this.db.SaveChanges();
        }

        public void RemoveFollow(int creatorId)
        {
            var person = this.personService.GetCurentPerson();
            var currentPerson = this.db.Persons.Find(person.Id);
            var creator = this.GetFeatureMixCreator(creatorId);
            if (currentPerson != null)
            {
                currentPerson.FollowedMixCreators.Remove(creator);
                this.db.Persons.AddOrUpdate(currentPerson);
            }

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