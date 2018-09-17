using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Services.Account;
using smartHookah.Models;
using smartHookah.Services.Person;

namespace smartHookah.Services.Gear
{
    public class TobaccoService : ITobaccoService
    {
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
            if(tobacco == null) throw new ItemNotFoundException($"Tobacco with id {id} not found.");
            return tobacco;
        }

        public PipeAccesoryStatistics GetTobaccoStatistics(Tobacco tobacco)
        {
            return tobacco.Statistics;
        }

        public PipeAccesoryStatistics GetPersonTobaccoStatistics(Tobacco tobacco)
        {
            var person = personService.GetCurentPerson();
            if (person == null) throw new AccountNotFoundException();
            var sessions = person.SmokeSessions.Where(a => a.MetaData.TobaccoId == tobacco.Id).ToList();
            return sessions.Any() ? CalculateStatistics(sessions.ToList()) : null;
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

        public async Task<List<Models.SmokeSession>> GetTobaccoSessions(Tobacco tobacco, int pageSize = 10, int page = 0)
        {
            return await db.SmokeSessions
                .Where(a => a.MetaData.TobaccoId == tobacco.Id)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize).ToListAsync();
        }

        public async Task<List<TobaccoReview>> GetTobaccoReviews(Tobacco tobacco, int pageSize = 10, int page = 0)
        {
            return await db.SmokeSessions
                .Where(a => a.MetaData.TobaccoId == tobacco.Id)
                .Select(a => a.Review)
                .OrderByDescending(a => a.PublishDate)
                .Skip(page * pageSize)
                .Take(pageSize).ToListAsync();
        }

        #endregion

        #region TobaccoMixes

        public async Task<TobaccoMix> GetTobaccoMix(int id)
        {
            var mix = await db.TobaccoMixs.FirstOrDefaultAsync(a => a.Id == id);
            if (mix == null) throw new ItemNotFoundException($"Tobacco mix with id {id} not found.");
            return mix;
        }

        public async Task<PipeAccesoryStatistics> GetTobaccoMixStatistics(TobaccoMix mix)
        {
            var mixes =  await db.TobaccoMixs.Where(a => !a.Tobaccos.Except(mix.Tobaccos).Any()).Include(a => a.Statistics).ToListAsync();
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

        public Task<List<Models.SmokeSession>> GetTobaccoMixSessions(TobaccoMix mix, int count = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<TobaccoReview>> GetTobaccoMixReviews(TobaccoMix mix, int count = 10)
        {
            throw new NotImplementedException();
        }

        #endregion


        private PipeAccesoryStatistics CalculateStatistics(IEnumerable<Models.SmokeSession> session)
        {
            var result = new PipeAccesoryStatistics();
            
            var smokeSessions = session as Models.SmokeSession[] ?? session.ToArray();

            if (!smokeSessions.Any())
                return null;
            result.PufCount = smokeSessions.Average(a => a.Statistics.PufCount);
            result.PackType = smokeSessions.GroupBy(i => i.MetaData.PackType).OrderByDescending(j => j.Count()).Select(a => a.Key).First();
            result.BlowCount = smokeSessions.Average(b => b.Pufs.Count(puf => puf.Type == PufType.Out));
            result.SessionDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SessionDuration.TotalSeconds));
            result.SmokeDuration = TimeSpan.FromSeconds(smokeSessions.Average(a => a.Statistics.SmokeDuration.TotalSeconds));
            result.Used = smokeSessions.Count();
            result.Weight = smokeSessions.Average(a => a.MetaData.TobaccoWeight);

            var smokeSesinReview = smokeSessions.Where(a => a.Review != null).Select(a => a.Review).ToArray();

            if (smokeSesinReview.Any())
            {
                result.Overall = smokeSesinReview.Average(a => a.Overall);
                result.Taste = smokeSesinReview.Average(a => a.Taste);
                result.Smoke = smokeSesinReview.Average(a => a.Smoke);
                result.Quality = smokeSesinReview.Average(a => a.Quality);
            }
            return result;
        }
    }
}