using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Account;
using Microsoft.VisualStudio.Services.Common;
using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookah.Services.Person;
using smartHookah.Services.Redis;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using smartHookahCommon.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Services.Gear
{


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
            var query = person.OwnedPipeAccesories.Where(a => a.DeleteDate == null).Select(a => a.PipeAccesory);
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
                    throw new KeyNotFoundException($"Accessories of type \'{type}\' not found.");
            }

            return query.ToList();
        }

        public PipeAccesory GetPipeAccessory(int id)
        {
            var accessory = db.PipeAccesories.Find(id);
            if (accessory == null)
                throw new ManaException(ErrorCodes.AccessoryNotFound, $"Accessory with id {id} was not found");
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
                if (oldVote.Value == (int)value)
                    throw new DuplicateItemFoundException($"Accessory {accessory.Id} already has vote value of \'{value.ToString()}\' from current person.");
                if (oldVote.Value == -1 && (int)value > oldVote.Value)
                {
                    accessory.DisLikeCount--;
                    accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                }
                else if (oldVote.Value == 1 && (int)value < oldVote.Value)
                {
                    accessory.LikeCount--;
                    accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
                }
                else if (oldVote.Value == 0 && value != VoteValue.Unlike)
                {
                    accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                    accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
                }
                oldVote.Value = (int)value;
            }
            else
            {
                if (value == VoteValue.Unlike) throw new KeyNotFoundException("Cannot unlike accessory that has never been liked/disliked.");
                accessory.LikeCount += value == VoteValue.Like ? 1 : 0;
                accessory.DisLikeCount += value == VoteValue.Dislike ? 1 : 0;
            }

            var vote = oldVote ?? new PipeAccesoryLike()
            {
                PersonId = person.Id,
                PipeAccesoryId = accessory.Id,
                Value = (int)value
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

        public List<SearchPipeAccessory> SearchAccessories(
            string search,
            AccesoryType type,
            SearchType searchType,
            int page,
            int pageSize)
        {
            page = page + 1;
            var query = searchType == SearchType.All ? ResourceHelper.ReadResources("smartHookah.Queries.searchType.sql") : ResourceHelper.ReadResources("smartHookah.Queries.searchByBrand.sql");
            var userId = this.personService.GetCurentPerson().Id;
            var result = this.db.Database.SqlQuery<SearchPipeAccessory>(
                query
                , new SqlParameter("personId", userId)
                , new SqlParameter("sp", search)
                , new SqlParameter("type", type.GetSearchName())
                , new SqlParameter("PageNumber", page),
                new SqlParameter("RowspPage", pageSize)).ToList();

            return result;
        }

        public ICollection<Models.Db.SmokeSession> UsedInSession(int accessoryId, int pageSize, int page)
        {
            var accessory = this.GetPipeAccessory(accessoryId);

            var person = this.personService.GetCurentPerson();

            var sessions = this.db.SmokeSessions.Include(a => a.MetaData).Include(a => a.Persons).Where(a =>
                a.MetaData != null &&
                (a.MetaData.TobaccoId == accessoryId
                 || a.MetaData.BowlId == accessoryId
                 || a.MetaData.TobaccoId == accessoryId
                 || a.MetaData.PipeId == accessoryId
                 || a.MetaData.HeatManagementId == accessoryId
                 || a.MetaData.CoalId == accessoryId)).Where(a => a.Persons.Any(p => p.Id == person.Id)).OrderByDescending(a => a.Id).Skip(pageSize * page).Take(pageSize);

            return sessions.ToList();
        }

        public async Task<Brand> GetBrand(string brandName)
        {
            var brand = await this.db.Brands.FindAsync(brandName);

            return brand;
        }

        public async Task<PipeAccesory> AddGear(PipeAccesory accessory)
        {
            var tryFindMatch = await this.db.PipeAccesories.Where(a => a.BrandName == accessory.BrandName && a.AccName.Equals(accessory.AccName) && a.Status == AccessoryStatus.Ok).FirstOrDefaultAsync();
            if (tryFindMatch != null)
            {
                return tryFindMatch;
            }

            var findBrand = await this.db.Brands.Where(a => a.DisplayName == accessory.BrandName || a.Name == accessory.BrandName).FirstOrDefaultAsync() ??
                            CreateBrandFromAccessory(accessory);
            var person = this.personService.GetCurentPerson(db);
            accessory.Brand = findBrand;
            accessory.BrandName = findBrand.Name;
            accessory.Status = AccessoryStatus.Waiting;
            accessory.CreatorId = person.Id;
            accessory.CreatedAt = DateTimeOffset.UtcNow;
            accessory.Id = 0;

            person.OwnedPipeAccesories.Add(new OwnPipeAccesories()
            {
                PipeAccesory = accessory,
                CreatedDate = DateTime.UtcNow,
                Person = person
            });

            this.db.Persons.AddOrUpdate(person);

            this.db.PipeAccesories.Add(accessory);
            this.db.SaveChanges();
            return accessory;
        }

        public async Task<PipeAccesory> MergeGear(int targetId, int sourceId)
        {
            var source = await this.db.PipeAccesories.FindAsync(sourceId);

            var target = await this.db.PipeAccesories.FindAsync(targetId);
            if (source == null || target == null)
            {
                throw new ManaException(ErrorCodes.PipeAccessoryNotFound);
            }

            //sessions
            var sessions = await this.db.SessionMetaDatas.Where(a =>
                a != null &&
                (a.TobaccoId == source.Id
                 || a.BowlId == source.Id
                 || a.PipeId == source.Id
                 || a.HeatManagementId == source.Id
                 || a.CoalId == source.Id)).ToListAsync();


            foreach (var smokeSession in sessions)
            {
                if (smokeSession.BowlId == source.Id)
                {
                    smokeSession.BowlId = target.Id;
                }
                if (smokeSession.PipeId == source.Id)
                {
                    smokeSession.PipeId = target.Id;
                }
                if (smokeSession.HeatManagementId == source.Id)
                {
                    smokeSession.HeatManagementId = target.Id;
                }
                if (smokeSession.CoalId == source.Id)
                {
                    smokeSession.CoalId = target.Id;
                }
                this.db.SessionMetaDatas.AddOrUpdate(smokeSession);
            }

            //own

            var ownage = await this.db.OwnPipeAccesorieses.Where(a => a.PipeAccesoryId == target.Id).ToListAsync();
            foreach (var ownPipeAccessoriese in ownage)
            {
                ownPipeAccessoriese.PipeAccesoryId = target.Id;
                this.db.OwnPipeAccesorieses.AddOrUpdate(ownPipeAccessoriese);
            }

            // media

            //likes
            var likes = await this.db.PipeAccesoryLikes.Where(a => a.PipeAccesoryId == source.Id).ToListAsync();
            foreach (var pipeAccessoryLike in likes)
            {
                pipeAccessoryLike.PipeAccesoryId = target.Id;
                this.db.PipeAccesoryLikes.AddOrUpdate(pipeAccessoryLike);
            }

            // reviews
            var reviews = await this.db.PipeAccessoryReviews.Where(a => a.AccessorId == source.Id).ToListAsync();
            foreach (var review in reviews)
            {
                review.AccessorId = target.Id;
                this.db.PipeAccessoryReviews.AddOrUpdate(review);
            }



            // this.db.SaveChanges();
            return target;

        }

        private Brand CreateBrandFromAccessory(PipeAccesory accessory)
        {
            var brand = new Brand
            {
                DisplayName = accessory.BrandName,
                Name = accessory.BrandName.OnlyAlphaNumeric(),
                Url = "auto-created"
            };

            switch (accessory.GetTypeEnum())
            {
                case AccesoryType.Bowl:
                    brand.Bowl = true;
                    break;

                case AccesoryType.Coal:
                    brand.Coal = true;
                    break;

                case AccesoryType.Heatmanagement:
                    brand.HeatManagment = true;
                    break;

                case AccesoryType.Tobacco:
                    brand.Tobacco = true;
                    break;

                case AccesoryType.Hookah:
                    brand.Tobacco = true;
                    break;
            }

            this.db.Brands.Add(brand);
            return brand;

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

            if (accessory == null) throw new KeyNotFoundException($"Accessory id {id} not found.");

            if (person.OwnedPipeAccesories.Any(a => a.PipeAccesory.Id == accessory.Id))
            {
                var current = person.OwnedPipeAccesories.FirstOrDefault(a => a.PipeAccesory.Id == accessory.Id && a.DeleteDate == null);
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

            if (accessory == null) throw new ManaException(ErrorCodes.AccessoryNotFound, $"Accessory id {id} not found.");

            if (person.OwnedPipeAccesories.All(a => a.PipeAccesory.Id != accessory.Id))
                throw new KeyNotFoundException($"OwnAccessory id {id} not found.");
            {
                var current = person.OwnedPipeAccesories.FirstOrDefault(a => a.PipeAccesory.Id == accessory.Id && a.DeleteDate == null);
                if (current == null) throw new KeyNotFoundException($"OwnAccessory id {id} not found.");
                //if (count != null && current.Amount > count)
                //{
                //    current.Amount -= (decimal) count;
                //    db.OwnPipeAccesorieses.AddOrUpdate(current);
                //    await db.SaveChangesAsync();
                //    return true;
                //}

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
                .Take(count)
                .ToList();

            var result = new List<PipeAccesory>();

            var bowls = sessions.Select(a => a.MetaData.Bowl as PipeAccesory).Where(a => a != null).ToList();
            var hmds = sessions.Select(a => a.MetaData.HeatManagement as PipeAccesory).Where(a => a != null).ToList();
            var pipes = sessions.Select(a => a.MetaData.Pipe as PipeAccesory).Where(a => a != null).ToList();
            var coals = sessions.Select(a => a.MetaData.Coal as PipeAccesory).Where(a => a != null).ToList();
            var tobacco = sessions.Where(a => !(a.MetaData.Tobacco is TobaccoMix)).Select(a => a.MetaData.Tobacco as PipeAccesory).Where(a => a != null).ToList();

            result.AddRangeIfRangeNotNull(bowls);
            result.AddRangeIfRangeNotNull(hmds);
            result.AddRangeIfRangeNotNull(pipes);
            result.AddRangeIfRangeNotNull(coals);
            result.AddRangeIfRangeNotNull(tobacco);

            return result.GroupBy(a => a.Id).Select(a => a.First()).ToList();
        }

        public Task<decimal> TobaccoEstimate(int sessionId)
        {
            throw new NotImplementedException();
        }
    }

}