using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Services.Device;
using smartHookah.Services.Redis;
using smartHookah.Services.SmokeSession;

namespace smartHookah.Mappers.ViewModelMappers.Person
{
    public class PersonIndexViewModelMapper : IPersonIndexViewModelMapper
    {
        private readonly SmartHookahContext db;
        private readonly IDeviceService deviceService;
        private readonly ISmokeSessionService sessionService;
        private readonly IRedisService redisService;
        

        public PersonIndexViewModelMapper(IDeviceService deviceService, SmartHookahContext db, IRedisService redisService, ISmokeSessionService sessionService)
        {
            this.deviceService = deviceService;
            this.db = db;
            this.redisService = redisService;
            this.sessionService = sessionService;
        }

        public async Task<PersonIndexViewModel> Map(Models.Db.Person person)
        {
            var result = new PersonIndexViewModel();
            var sessions = person.SmokeSessions.Where(a => a.Statistics != null).OrderByDescending(a => a.Statistics.Start).Take(5);

            result.SmokeSessions = sessions.ToList();
            var hookahs = person.Hookahs.ToList();
            var onlineState = await this.deviceService.GetOnlineStates(hookahs.Select(a => a.Code));
            result.OnlineHookah = onlineState.Where(a => a.Value).Select(a => a.Key).ToList();
            var todayDate = DateTime.Now.Date;
            result.ActiveReservations = db.Reservations.Where(a => a.Customers.Any(p => p.Id == person.Id)
                                                                 && a.Status != ReservationState.Canceled
                                                                 && DbFunctions.TruncateTime(a.Time) >= todayDate).ToList();

            result.Hookah = hookahs;
            result.ActiveSession = db.SmokeSessions.Where(a =>
                a.Persons.Any(p => p.Id == person.Id) && a.Statistics == null).ToList();


            result.Person = person;
            var activeHookah = result.ActiveSession.Select(a => a.Hookah).Union(hookahs).ToList();
            result.DynamicStatistic = this.sessionService.GetDynamicSmokeStatistics(activeHookah, a =>  this.redisService.GetSessionId(a.Code));

            return result;
        }
    }
}