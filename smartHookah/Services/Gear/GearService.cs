using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;
using ClosedXML.Excel;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Common;
using smartHookah.Models;
using smartHookah.Services.Person;

namespace smartHookah.Services.Gear
{
    using System.Data.SqlClient;

    using smartHookah.Helpers;
    using smartHookah.Models.Dto;
    using smartHookah.Services.Redis;

    public partial class GearService : IGearService
    {
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        private readonly ICacheService cacheService;

        public GearService(SmartHookahContext db, IPersonService personService, ICacheService cacheService)
        {
            this.db = db;
            this.personService = personService;
            this.cacheService = cacheService;
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

        public Dictionary<AccesoryType, List<BrandGroupDto>> GetBrands()
        {
            var cachedResult = this.cacheService.Get<Dictionary<AccesoryType, List<BrandGroupDto>>>("brands");

            if (cachedResult != null)
            {
                return cachedResult;
            }

            var brands = this.db.PipeAccesories.Include("Brand").ToList();

            var groupedBrands = brands.GroupBy(a => a.GetTypeEnum());

            var result = groupedBrands.ToDictionary(
                k => k.Key,
                v => v.Select(s => s.Brand).OrderBy(a => a.DisplayName).Distinct().Select(
                    b => new BrandGroupDto
                             {
                             Id = b.Name,
                             Name = b.DisplayName,
                             Picture = b.Picture,
                             ItemCount = v.Count(a => a.BrandName == b.Name)
                         }).ToList());

            this.cacheService.Store("brands", result);

            return result;

        }

        public List<Models.Dto.GearService.SearchPipeAccesory> SearchAccesories(
            string search,
            AccesoryType type,
            int page,
            int pageSize)
        {
            page = page + 1;
            var query = ResourceHelper.ReadResources("smartHookah.Queries.searchType.sql");
            var userId = this.personService.GetCurentPerson().Id;
            var result = this.db.Database.SqlQuery<Models.Dto.GearService.SearchPipeAccesory>(
                query
                , new SqlParameter("personId", userId)
                , new SqlParameter("sp", search)
                , new SqlParameter("type", type.GetSearchName())
                , new SqlParameter("PageNumber", page),
                new SqlParameter("RowspPage", pageSize)).ToList();

            return result;
        }

        public int UsedByPerson(PipeAccesory accessory, int personId)
        {
            var person = personService.GetCurentPerson(personId);

            var result = db.Persons.FirstOrDefault(a => a.Id == person.Id)?.SmokeSessions?
                .Count(a => a.MetaData.Bowl?.Id == accessory.Id || 
                            a.MetaData.Pipe?.Id == accessory.Id || 
                            a.MetaData.HeatManagement?.Id == accessory.Id || 
                            a.MetaData.Coal?.Id == accessory.Id);

            return result ?? 0;
        }

        public int UsedByPerson(PipeAccesory accessory)
        {
            var person = personService.GetCurentPerson();
            
            var result = db.Persons.FirstOrDefault(a => a.Id == person.Id)?.SmokeSessions?
                .Count(a => a.MetaData.Bowl?.Id == accessory.Id ||
                            a.MetaData.Pipe?.Id == accessory.Id ||
                            a.MetaData.HeatManagement?.Id == accessory.Id ||
                            a.MetaData.Coal?.Id == accessory.Id);

            return result ?? 0;
        }

        public IDictionary<PipeAccesory, int> UsedWith(PipeAccesory accessory)
        {
            var sessions = db.SmokeSessions
                .Include(a => a.MetaData)
                .Where(a => a.MetaData.Bowl.Id == accessory.Id ||
                            a.MetaData.Pipe.Id == accessory.Id ||
                            a.MetaData.HeatManagement.Id == accessory.Id ||
                            a.MetaData.Coal.Id == accessory.Id).ToList();
            var result = new Dictionary<PipeAccesory, int>();

            var bowls = (from o in sessions
                where o.MetaData.Bowl != null
                where o.MetaData.Bowl.Id != accessory.Id
                group o by o.MetaData.Bowl).ToDictionary(k => k.Key as PipeAccesory, v => v.Count());

            var pipes = (from o in sessions
                where o.MetaData.Pipe != null
                where o.MetaData.Pipe.Id != accessory.Id
                group o by o.MetaData.Pipe).ToDictionary(k => k.Key as PipeAccesory, v => v.Count());

            var hmds = (from o in sessions
                where o.MetaData.HeatManagement != null
                where o.MetaData.HeatManagement.Id != accessory.Id
                group o by o.MetaData.HeatManagement).ToDictionary(k => k.Key as PipeAccesory, v => v.Count());

            var coals = (from o in sessions
                where o.MetaData.Coal != null
                where o.MetaData.Coal.Id != accessory.Id
                group o by o.MetaData.Coal).ToDictionary(k => k.Key as PipeAccesory, v => v.Count());
            
            result.AddRangeIfRangeNotNull(bowls);
            result.AddRangeIfRangeNotNull(hmds);
            result.AddRangeIfRangeNotNull(pipes);
            result.AddRangeIfRangeNotNull(coals);

            return result;
        }

        public int OwnedByPersons(PipeAccesory accessory)
        {
            var result = db.Persons.Count(a => !a.Places.Any() && a.OwnedPipeAccesories.Any(x => x.PipeAccesory.Id == accessory.Id));
            return result;
        }

        public int OwnedByPlaces(PipeAccesory accessory)
        {
            var result = db.Persons.Count(a => a.Places.Any() && a.OwnedPipeAccesories.Any(x => x.PipeAccesory.Id == accessory.Id));
            return result;
        }

        public async Task<PipeAccesory> AddMyGear(int id, int count, int? personId)
        {
            var person = personId == null
                ? personService.GetCurentPerson()
                : personService.GetCurentPerson(personId);

            var accessory = db.PipeAccesories.FirstOrDefault(a => a.Id == id);

            if (accessory == null) throw new ItemNotFoundException($"Accessory id {id} not found.");

            if (person.OwnedPipeAccesories.Any(a => a.PipeAccesory.Id == accessory.Id))
            {
                var current = person.OwnedPipeAccesories.FirstOrDefault(a => a.PipeAccesory.Id == accessory.Id);
                if (current != null)
                {
                    current.Amount += count;
                    db.OwnPipeAccesorieses.AddOrUpdate(current);
                    await db.SaveChangesAsync();
                    return accessory;
                }
            }
            db.OwnPipeAccesorieses.Add(new OwnPipeAccesories()
            {
                Amount = count,
                PipeAccesoryId = accessory.Id,
                PersonId = person.Id,
                CreatedDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
            return accessory;
        }

        public async Task<bool> DeleteMyGear(int id, int? count, int? personId)
        {
            var person = personId == null
                ? personService.GetCurentPerson()
                : personService.GetCurentPerson(personId);

            var accessory = db.PipeAccesories.FirstOrDefault(a => a.Id == id);

            if (accessory == null) throw new ItemNotFoundException($"Accessory id {id} not found.");

            if (person.OwnedPipeAccesories.All(a => a.PipeAccesory.Id != accessory.Id))
                throw new ItemNotFoundException($"OwnAccessory id {id} not found.");
            {
                var current = person.OwnedPipeAccesories.FirstOrDefault(a => a.PipeAccesory.Id == accessory.Id);
                if (current == null) throw new ItemNotFoundException($"OwnAccessory id {id} not found.");
                if (count != null && current.Amount > count)
                {
                    current.Amount -= (decimal) count;
                    db.OwnPipeAccesorieses.AddOrUpdate(current);
                    await db.SaveChangesAsync();
                    return true;
                }

                current.Amount = 0;
                current.DeleteDate = DateTime.UtcNow;
                db.OwnPipeAccesorieses.AddOrUpdate(current);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public ICollection<PipeAccesory> GetRecentAccessories(int count)
        {
            var person = personService.GetCurentPerson();

            var sessions = person.SmokeSessions
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();

            var result = new List<PipeAccesory>();

            var bowls = sessions.Select(a => a.MetaData.Coal as PipeAccesory).Where(a => a != null).ToList();
            var hmds = sessions.Select(a => a.MetaData.HeatManagement as PipeAccesory).Where(a => a != null).ToList();
            var pipes = sessions.Select(a => a.MetaData.Pipe as PipeAccesory).Where(a => a != null).ToList();
            var coals = sessions.Select(a => a.MetaData.Coal as PipeAccesory).Where(a => a != null).ToList();

            result.AddRangeIfRangeNotNull(bowls);
            result.AddRangeIfRangeNotNull(hmds);
            result.AddRangeIfRangeNotNull(pipes);
            result.AddRangeIfRangeNotNull(coals);

            return result.GroupBy(a => a.Id).Select(a => a.First()).ToList();
        }
    }

}