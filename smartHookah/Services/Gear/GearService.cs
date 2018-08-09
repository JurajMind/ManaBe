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
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        public GearService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public List<PipeAccesory> GetPersonAccessories(int personId, string type)
        {
            var person = db.Persons.FirstOrDefault(a => a.Id == personId);
            if (person == null) throw new AccountNotFoundException();
            var result = type == "All"
                ? person.OwnedPipeAccesories.Select(a => a.PipeAccesory).ToList()
                : person.OwnedPipeAccesories.Select(a => a.PipeAccesory).Where(a => a.GetTypeName() == type).ToList();
            if(result == null) throw new ItemNotFoundException($"Accessories of type \'{type}\' not found.");
            return result;
        }

        public PipeAccesory GetPipeAccessory(int id)
        {
            var accessory = db.PipeAccesories.Find(id);
            if(accessory == null) throw new ItemNotFoundException($"Accessory with id {id} not found.");
            return accessory;
        }

        public void Vote(int id, VoteValue value)
        {
            var accessory = GetPipeAccessory(id);
            var person = personService.GetCurentPerson();
            if (person == null) throw new AccountNotFoundException();
            var oldVote = db.PipeAccesoryLikes.Where(a => a.PersonId == person.Id).FirstOrDefault(a => a.PipeAccesoryId == accessory.Id);
            if (oldVote != null)
            {
                if(oldVote.Value == (int)value) 
                    throw new DuplicateItemFoundException($"Accessory {accessory.Id} already has vote value of \'{value.ToString()}\' from current person.");
                if (oldVote.Value == -1 && (int) value > oldVote.Value)
                {
                    accessory.DisLikeCount--;
                    accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                }
                else if (oldVote.Value == 1 && (int) value < oldVote.Value)
                {
                    accessory.LikeCount--;
                    accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
                }
                else if (oldVote.Value == 0 && value != VoteValue.Unlike)
                {
                    accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                    accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
                }
                oldVote.Value = (int) value;
            }
            else
            {
                if (value == VoteValue.Unlike) throw new ItemNotFoundException("Cannot unlike accessory that has never been liked/disliked.");
                accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
            }

            var vote = oldVote ?? new PipeAccesoryLike()
            {
                PersonId = person.Id,
                PipeAccesoryId = accessory.Id,
                Value = (int) value
            };
            
            accessory.Likes.Add(vote);
            db.PipeAccesoryLikes.AddOrUpdate(vote);
            db.PipeAccesories.AddOrUpdate(accessory);
            db.SaveChanges();
        }
    }
}