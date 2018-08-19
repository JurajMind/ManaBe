using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Account;
using PInvoke;
using smartHookah.Models;
using smartHookah.Services.Person;

namespace smartHookah.Services.Gear
{
    using System.Data.SqlClient;

    using smartHookah.Helpers;

    public partial class GearService : IGearService
    {
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        public GearService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public List<PipeAccesory> GetPersonAccessories(int? personId, string type)
        {
            var person = personId == null 
                ? personService.GetCurentPerson()
                : db.Persons.Find(personId);
            
            if (person == null) throw new AccountNotFoundException();
            var query = person.OwnedPipeAccesories.Select(a => a.PipeAccesory);
            switch (type.ToLower())
            {
                case "bowl":
                    query = query.Where(a => a is Bowl);
                    break;
                case "tobacco":
                    query = query.Where(a => a is Tobacco);
                    break;
                case "heatmanagement":
                    query = query.Where(a => a is HeatManagment);
                    break;
                case "hookah":
                    query = query.Where(a => a is Pipe);
                    break;
                case "coal":
                    query = query.Where(a => a is Coal);
                    break;
                case "all":
                    break;
                default:
                    throw new ItemNotFoundException($"Accessories of type \'{type}\' not found.");
            }

            return query.ToList();
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

        public List<Models.Dto.GearService.SearchPipeAccesory> SearchAccesories(string search, AccesoryType type,int page,int pageSize)
        {
            page = page + 1;
            var query = ResourceHelper.ReadResources("smartHookah.Queries.searchType.sql");
            var userId = this.personService.GetCurentPerson().Id;
            var result = this.db.Database.SqlQuery<Models.Dto.GearService.SearchPipeAccesory>(
                query
                , new SqlParameter("personId", userId)
                , new SqlParameter("sp", search)
                , new SqlParameter("type", type.GetName())
                , new SqlParameter("PageNumber", page)
                , new SqlParameter("RowspPage", pageSize)
                ).ToList();

            return result;
        }

        
    }

}