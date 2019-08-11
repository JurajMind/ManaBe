using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookah.Services.Person;

namespace smartHookah.Services.Statistics
{
    public class PersonStatisticsService : IPersonStatisticsService
    {
        private readonly SmartHookahContext db;
        private readonly IPersonService personService;

        public PersonStatisticsService(IPersonService personService, SmartHookahContext db)
        {
            this.personService = personService;
            this.db = db;
        }

        public List<Models.Db.SmokeSession> GetPersonSessions(DateTime? from, DateTime? to)
        {
            var person = personService.GetCurentPerson();
            if (person == null)
            {
                throw new NotAuthorizedException("User not authorized.");
            }

            return db.SmokeSessions
                .Where(a => a.Persons.Any(x => x.Id == person.Id))
                .Where(a => a.Statistics.Start >= from && a.Statistics.End <= to)
                .Include(a => a.MetaData)
                .Include(a => a.Statistics)
                .OrderByDescending(a => a.Statistics.Start)
                .ToList();
        }

        public SmokeSessionTimeStatisticsDto GetSessionTimeStatistics(IEnumerable<Models.Db.SmokeSession> sessions)
        {
            var result = new SmokeSessionTimeStatisticsDto();
            foreach (var session in sessions)
            {
                result.SessionsCount++;
                result.SessionsDuration += session.Statistics.SessionDuration;
                result.SmokeDuration += session.Statistics.SmokeDuration;
                result.DayOfWeekDistribution[(int)session.Statistics.Start.DayOfWeek]++;
                var hour = GetHour(session.Statistics.Start);
                if (result.SessionStartTimeDistribution.Keys.Any(a => a == hour))
                {
                    result.SessionStartTimeDistribution[hour]++;
                }
                else
                {
                    result.SessionStartTimeDistribution.Add(hour, 1);
                }

                result.PuffCount += session.Statistics.PufCount;
            }

            return result;
        }

        public List<PipeAccessoryUsageDto> GetAccessoriesUsage(IEnumerable<Models.Db.SmokeSession> sessions)
        {
            var person = personService.GetCurentPerson();
            if (person == null)
            {
                throw new NotAuthorizedException("User not authorized.");
            }

            var result = new List<PipeAccessoryUsageDto>();
            foreach (var session in sessions)
            {
                AddAccessoryToUsage(person, result, session, smokeSession => smokeSession.MetaData.Bowl);
                AddAccessoryToUsage(person, result, session, smokeSession => smokeSession.MetaData.Coal);
                AddAccessoryToUsage(person, result, session, smokeSession => smokeSession.MetaData.HeatManagement);
                AddAccessoryToUsage(person, result, session, smokeSession => smokeSession.MetaData.Pipe);
                AddTobaccoUssage(person, result, session);
            }

            return result;
        }

        private static void AddAccessoryToUsage(Models.Db.Person person, List<PipeAccessoryUsageDto> result, Models.Db.SmokeSession session, Func<Models.Db.SmokeSession, PipeAccesory> getTypedAccesory)
        {
            if (getTypedAccesory(session) == null) return;
            if (result.All(a => a.Id != getTypedAccesory(session).Id))
            {
                var ownedAccessories = person.OwnedPipeAccesories.ToList();
                var bowl = new PipeAccessoryUsageDto()
                {
                    Id = getTypedAccesory(session).Id,
                    Type = getTypedAccesory(session).GetTypeName(),
                    AccName = getTypedAccesory(session).AccName,
                    BrandName = getTypedAccesory(session).Brand.DisplayName,
                    Used = 1,
                    Owned = ownedAccessories.Any(a => a.PipeAccesoryId == getTypedAccesory(session).Id)
                };
                result.Add(bowl);
            }
            else
            {
                result.Find(a => a.Id == getTypedAccesory(session).Id).Used++;
            }
        }

        private static void AddTobaccoUssage(Models.Db.Person person, List<PipeAccessoryUsageDto> result, Models.Db.SmokeSession session)
        {
            List<Tobacco> usedTobaccos = new List<Tobacco>();
            var inMix = false;
            if (session.MetaData.Tobacco is TobaccoMix mix)
            {
                usedTobaccos.AddRange(mix.Tobaccos.Select(s => s.Tobacco));
                inMix = true;
            }
            else
            {
                if (session.MetaData.Tobacco != null)
                {
                    usedTobaccos.Add(session.MetaData.Tobacco);
                }
            }

            foreach (var usedTobacco in usedTobaccos)
            {

                if (result.All(a => a.Id != usedTobacco.Id))
                {
                    var ownedAccessories = person.OwnedPipeAccesories.ToList();
                    var bowl = new PipeAccessoryUsageDto()
                    {
                        Id = usedTobacco.Id,
                        Type = usedTobacco.GetTypeName(),
                        AccName = usedTobacco.AccName,
                        BrandName = usedTobacco.Brand.DisplayName,
                        Used = 1,
                        Owned = ownedAccessories.Any(a => a.PipeAccesoryId == usedTobacco.Id),
                        InMix = inMix ? 1 : 0

                    };
                    result.Add(bowl);
                }
                else
                {
                    var match = result.Find(a => a.Id == usedTobacco.Id);
                    match.Used++;
                    if (inMix)
                    {
                        match.InMix++;
                    }
                }
            }
        }

        private static int GetHour(DateTime time) => time.Minute >= 45 ? time.Hour + 1 : time.Hour;
    }
}