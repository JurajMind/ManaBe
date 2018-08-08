using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Account;
using smartHookah.Models;
using smartHookah.Services.Person;

namespace smartHookah.Services.Gear
{
    public class GearService : IGearService
    {
        private readonly SmartHookahContext _db;
        private readonly IPersonService _personService;

        public GearService(SmartHookahContext db, IPersonService personService)
        {
            _db = db;
            _personService = personService;
        }

        public List<PipeAccesory> GetPersonAccessories(int personId)
        {
            var person = _db.Persons.FirstOrDefault(a => a.Id == personId);
            if (person == null) throw new AccountNotFoundException();
            return person?.OwnedPipeAccesories.Select(a => a.PipeAccesory).ToList();
        }

        public PipeAccesory GetPipeAccessory(int id)
        {
            var accessory = _db.PipeAccesories.Find(id);
            if(accessory == null) throw new ItemNotFoundException($"Accessory with id {id} not found.");
            return accessory;
        }

        public void Vote(int id, VoteValue value)
        {
            var accessory = GetPipeAccessory(id);
            var person = _personService.GetCurentPerson();
            if (person == null) throw new AccountNotFoundException();
            var oldVote = _db.PipeAccesoryLikes.Where(a => a.PersonId == person.Id).FirstOrDefault(a => a.PipeAccesoryId == accessory.Id);
            if (oldVote != null) oldVote.Value = (int) value;

            var vote = oldVote ?? new PipeAccesoryLike()
            {
                PersonId = person.Id,
                PipeAccesoryId = accessory.Id,
                Value = (int) value
            };

            _db.PipeAccesoryLikes.AddOrUpdate(vote);
            _db.SaveChanges();
        }
    }
}