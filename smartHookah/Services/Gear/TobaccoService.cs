﻿using Microsoft.VisualStudio.Services.Account;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;
using smartHookah.Services.Person;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Services.Gear
{
    using log4net;
    using System.Data.Entity.Migrations;
    using System.Web.Helpers;

    public class TobaccoService : ITobaccoService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(TobaccoService));
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        public TobaccoService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        #region Tobaccos

        public async Task<Tobacco> GetTobacco(int id)
        {
            var tobacco = await db.Tobaccos.FirstOrDefaultAsync(a => a.Id == id);
            if (tobacco == null) throw new KeyNotFoundException($"Tobacco with id {id} not found.");
            return tobacco;
        }

        public PipeAccesoryStatistics GetTobaccoStatistics(Tobacco tobacco)
        {
            return tobacco.Statistics;
        }

        public PipeAccesoryStatistics GetPersonTobaccoStatistics(Tobacco tobacco)
        {
            var person = personService.GetCurentPersonId();
            if (person == null) throw new AccountNotFoundException();
            var sessions = db.SmokeSessions.Include(a => a.Statistics).Include(a => a.SessionReview).Where(a => a.MetaData != null && a.MetaData.TobaccoId == tobacco.Id && a.Persons.Any(p => p.Id == person)).ToList();
            return sessions.Any() ? CalculateStatistics(sessions) : null;
        }

        [Authorize(Roles = "Admin")]
        public async Task<PipeAccesoryStatistics> GetPersonTobaccoStatistics(Tobacco tobacco, int personId)
        {
            var person = await db.Persons.FirstOrDefaultAsync(a => a.Id == personId);
            if (person == null) throw new AccountNotFoundException();
            var sessions = person.SmokeSessions.Where(a => a.MetaData.TobaccoId == tobacco.Id).ToList();
            return sessions.Any() ? CalculateStatistics(sessions.ToList()) : null;
        }

        public List<TobaccoTaste> GetTobaccoTastes(Tobacco tobacco)
        {
            return tobacco.Tastes.ToList();
        }

        public async Task<List<Models.Db.SmokeSession>> GetTobaccoSessions(Tobacco tobacco, int pageSize = 10, int page = 0)
        {
            var person = this.personService.GetCurentPersonId();
            return await db.SmokeSessions
                .Where(a => a.MetaData != null && a.MetaData.TobaccoId == tobacco.Id && a.Persons.Any(p => p.Id == person))
                .OrderByDescending(a => a.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize).ToListAsync();
        }

        public IList<Tobacco> GetTobaccoList(int page = 1, int pageSize = 50, TobaccoFilter filter = null)
        {
            if (filter == null)
            {
                filter = new TobaccoFilter { SortDirection = SortDirection.Ascending, SortBy = TobaccoSortBy.Smart };
            }
            var filteredTobacco = this.GetFilteredTobacco(filter);

            var skip = (page - 1) * pageSize;

            return filteredTobacco.OrderBy(a => a.Id).Skip(skip).Take(pageSize).ToList();
        }

        private IQueryable<Tobacco> GetFilteredTobacco(TobaccoFilter filter)
        {
            var tobacco = this.db.Tobaccos.Include("Statistics").AsQueryable();
            if (filter == null)
            {
                return tobacco;
            }
            var person = this.personService.GetCurentPerson();

            if (filter.Owned)
            {
                tobacco = person.Tobaccos.AsQueryable();
            }

            if (filter.Smoked)
            {
                tobacco = person.SmokeSessions.Where(a => a.MetaData != null && a.MetaData.Tobacco != null)
                    .Select(a => a.MetaData.Tobacco).AsQueryable();
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                tobacco = tobacco.Where(t => t.BrandName == filter.Brand);
            }

            if (filter.Tastes != null && filter.Tastes.Any())
            {
                tobacco = tobacco.Where(a => a.Tastes.All(t => filter.Tastes.Contains(t.Id)));
            }


            return tobacco;
        }

        #endregion

        #region TobaccoMixes

        public async Task<TobaccoMix> GetTobaccoMix(int id)
        {
            var mix = await db.TobaccoMixs.FirstOrDefaultAsync(a => a.Id == id);
            if (mix == null) throw new KeyNotFoundException($"Tobacco mix with id {id} not found.");
            return mix;
        }

        public async Task<List<TobaccoMix>> GetMixFromTobacco(int id, int pageSize = 10, int page = 0)
        {
            var mixes = await this.db.TobaccosMixParts.Where(a => a.TobaccoId == id && a.InMix != null).Select(a => a.InMix).OrderBy(a => a.Statistics.Used).Distinct().OrderBy(b => b.Id).Skip(page * pageSize)
                .Take(pageSize).ToListAsync();

            return mixes;
        }

        public async Task<List<TobaccoMix>> GetMixFromTobaccos(string name,List<int> ids, int pageSize = 10, int page = 0, bool union = true)
        {
            if (name != null && (ids == null || ids.Count == 0))
            {
                return this.db.TobaccoMixs.Where(a => a.AccName.ToUpper().Contains(name.ToUpper())).ToList();
            }

            var result = new List<TobaccoMix>();

            foreach (var id in ids)
            {
                result.AddRange(await this.GetMixFromTobacco(id, pageSize, page));
            }

            if(union)
                result = result.Where(m => ids.All( x => m.Tobaccos.Any( y => x == y.TobaccoId))).ToList();

            var filtered = result.GroupBy(a => a.Id).OrderBy(b => b.Count()).SelectMany(b => b).Distinct();

            if (name != null)
            {
                filtered = filtered.Where(a => a.AccName.ToUpper().Contains(name.ToUpper()));
            }
            return filtered.ToList();
        }

        public async Task<List<Tobacco>> SuggestTobaccos(List<int> ids, int pageSize = 10, int page = 0, bool own = true)
        {
            var mixes = await this.GetMixFromTobaccos(null,ids, pageSize, page);

            var tobaccos = mixes.SelectMany(a => a.Tobaccos).Select(a => a.Tobacco).Distinct().ToList();

            if (!own)
                return tobaccos;

            var owned = this.personService.GetCurentPerson().Tobaccos;


            return tobaccos.Intersect(owned).ToList();
        }

        public async Task<PipeAccesoryStatistics> GetTobaccoMixStatistics(TobaccoMix mix)
        {
            var mixes = await db.TobaccoMixs.Where(a => !a.Tobaccos.Except(mix.Tobaccos).Any()).Include(a => a.Statistics).ToListAsync();
            var sessions = await db.SmokeSessions.Where(a => mixes.Exists(x => x.Id == a.MetaData.TobaccoId)).ToListAsync();
            return CalculateStatistics(sessions);
        }

        public async Task<PipeAccesoryStatistics> GetPersonTobaccoMixStatistics(TobaccoMix mix)
        {
            var person = personService.GetCurentPerson();
            if (person == null) throw new AccountNotFoundException();
            var mixes = await db.TobaccoMixs.Where(a => !a.Tobaccos.Except(mix.Tobaccos).Any()).Include(a => a.Statistics).ToListAsync();
            var sessions = await db.SmokeSessions.Where(a => mixes.Exists(x => x.Id == a.MetaData.TobaccoId)).ToListAsync();
            return CalculateStatistics(sessions);
        }

        [Authorize(Roles = "Admin")]
        public async Task<PipeAccesoryStatistics> GetPersonTobaccoMixStatistics(TobaccoMix mix, int personId)
        {
            var person = personService.GetCurentPerson();
            if (person == null) throw new AccountNotFoundException();

            var mixes = await db.TobaccoMixs.Where(a => !a.Tobaccos.Except(mix.Tobaccos).Any()).Include(a => a.Statistics).ToListAsync();
            var sessions = await db.SmokeSessions.Where(a => mixes.Exists(x => x.Id == a.MetaData.TobaccoId)).ToListAsync();
            return CalculateStatistics(sessions);
        }

        public Dictionary<int, List<TobaccoTaste>> GetTobaccoMixTastes(TobaccoMix mix)
        {
            var tobaccos = mix.Tobaccos.Select(a => a.Tobacco);
            return tobaccos.ToDictionary(tobacco => tobacco.Id, GetTobaccoTastes);
        }

        public Task<List<Models.Db.SmokeSession>> GetTobaccoMixSessions(TobaccoMix mix, int count = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<TobaccoReview>> GetTobaccoMixReviews(TobaccoMix mix, int count = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<TobaccoMix> AddOrUpdateMix(TobaccoMix newMix)
        {
            if (newMix == null)
            {
                return null;
            }

            var originalMix = await this.db.TobaccoMixs.FindAsync(newMix.Id);
            newMix.CreatedAt = DateTimeOffset.UtcNow;
            var author = this.personService.GetCurentPerson();
            newMix.AuthorId = author.Id;
            newMix.BrandName = author.AssignedBrandId ?? "OwnBrand";
            if (this.MatchMixes(originalMix, newMix))
            {
                return originalMix;
            }
            if (originalMix != null)
            {
                newMix.Id = this.db.SessionMetaDatas.Count(a => a.TobaccoId == originalMix.Id) <= 1
                                ? originalMix.Id
                                : 0;
                var mixParts = originalMix.Tobaccos.ToList();

                this.db.TobaccosMixParts.RemoveRange(mixParts);
                foreach (var part in newMix.Tobaccos)
                {
                    part.InMixId = newMix.Id;
                }
                this.db.TobaccosMixParts.AddRange(newMix.Tobaccos);

            }
            else
            {
                newMix.Id = 0;
                newMix.Brand = this.db.Brands.Find(newMix.BrandName);
                foreach (var newMixTobacco in newMix.Tobaccos)
                {
                    newMixTobacco.Tobacco = await this.db.Tobaccos.FindAsync(newMixTobacco.TobaccoId);
                }
            }

            try
            {
                this.db.PipeAccesories.AddOrUpdate(newMix);
                await this.db.SaveChangesAsync();
                var test = await this.db.TobaccoMixs.FindAsync(newMix.Id);
                return test;
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                throw new Exception(
                    $"Unknown error", e);
            }
        }

        private bool MatchMixes(TobaccoMix originalMix, TobaccoMix newMix)
        {
            if (originalMix == null) return false;
            return newMix.Tobaccos.All(mixPart => originalMix.Tobaccos.Any(t => t.TobaccoId == mixPart.TobaccoId && t.Fraction == mixPart.Fraction)) && originalMix.Tobaccos.All(mixPart => newMix.Tobaccos.Any(t => t.TobaccoId == mixPart.TobaccoId && t.Fraction == mixPart.Fraction));
        }

        #endregion


        private PipeAccesoryStatistics CalculateStatistics(IEnumerable<Models.Db.SmokeSession> session)
        {
            var result = new PipeAccesoryStatistics();
            var personId = this.personService.GetCurentPerson().Id;
            var smokeSessions = session as Models.Db.SmokeSession[] ?? session.Where(a => a.Statistics != null).ToArray();

            if (!smokeSessions.Any())
                return null;
            result.PufCount = smokeSessions.Average(a => a.Statistics.PufCount);
            result.PackType = smokeSessions.GroupBy(i => i.MetaData.PackType).OrderByDescending(j => j.Count()).Select(a => a.Key).First();
            result.BlowCount = smokeSessions.Average(b => b.DbPufs.Count(puf => puf.Type == PufType.Out));
            result.SessionDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SessionDuration.TotalSeconds));
            result.SmokeDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SmokeDuration.TotalSeconds));
            result.Used = smokeSessions.Count();
            result.Weight = smokeSessions.Average(a => a.MetaData.TobaccoWeight);

            var smokeSesinReview = smokeSessions.Where(a => a.SessionReview != null && a.Review.AuthorId == personId).Select(a => a.Review).ToArray();

            if (smokeSesinReview.Any())
            {
                result.Overall = smokeSesinReview.Average(a => a.Overall);
                result.Taste = smokeSesinReview.Average(a => a.Taste);
                result.Smoke = smokeSesinReview.Average(a => a.Smoke);
                result.Cut = smokeSesinReview.Average(a => a.Cut);
                result.Duration = smokeSesinReview.Average(a => a.Duration);
                result.Strength = smokeSesinReview.Average(a => a.Strength);

            }
            return result;
        }
    }
}