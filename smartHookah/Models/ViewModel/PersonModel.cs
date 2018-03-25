using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Redis;

namespace smartHookah.Controllers
{
    public class MyStatisticViewModel
    {
        public List<SmokeSession> session;
    }

    public class PersonIndexViewModel
    {
        public List<SmokeSession> SmokeSessions { get; set; }
        public List<string> OnlineHookah { get; set; }
        public List<Hookah> Hookah { get; set; }
        public List<SmokeSession> ActiveSession { get; set; }
        public Dictionary<string, DynamicSmokeStatistic> DynamicStatistic { get; set; }

        public Person Person { get; set; }

        public async Task Fill(Models.Person person, SmartHookahContext db)
        {
            var sessions = person.SmokeSessions.Where(a => a.Statistics != null).OrderByDescending(a => a.Statistics.Start).Take(5);
            
            this.SmokeSessions = sessions.ToList();
            var hookahs = person.Hookahs.ToList();
            this.OnlineHookah = await IotDeviceHelper.GetState(hookahs.Select(a => a.Code).ToList());
            var todayDate = DateTime.Now.Date;
            this.ActiveReservations = db.Reservations.Where(a => a.Customers.Any(p => p.Id == person.Id)
                                                                 && a.Status != ReservationState.Canceled
                                                                 && DbFunctions.TruncateTime(a.Time) >= todayDate).ToList();

            this.Hookah = hookahs;
            this.ActiveSession = db.SmokeSessions.Where(a =>
                a.Persons.Any(p => p.Id == person.Id) && a.Statistics == null).ToList();


            this.Person = person;
            var activeHookah = this.ActiveSession.Select(a => a.Hookah).Union(hookahs).ToList();
            this.DynamicStatistic = SmokeSessionController.GetDynamicSmokeStatistic(activeHookah, a => a.SessionCode);
        }

        public IEnumerable<Reservation> ActiveReservations { get; set; }
    }

    public class ShowGearViewModel
    {
        public List<Tobacco> Tobaccos { get; set; }
        public List<Bowl> Bowls { get; set; }
        public List<Pipe> Pipes { get; set; }

        public Person Person { get; set; }
        public bool CanEdit { get; set; }
        public string DisplayName { get; set; }
    }

    public class DashBoardViewModel
    {
        public IQueryable<SmokeSession> Sessions { get; set; }


        public IQueryable<SmokeSession> LiveSessions { get; set; }
        public ApplicationUser User { get; set; }
        public List<SmokeSessionStatistics> SessionStatistics { get; set; }
        public List<SmokeSessionMetaData> MetaData { get; set; }
    }

    public class AddHookahPostModel
    {
        public int SelectedHookah { get; set; }
        public int PersonId { get; set; }
    }

    public class AddHookahViewModel
    {
        public Person Person { get; set; }
        public List<Hookah> Hookahs { get; set; }

        public int SelectedHookah { get; set; }
    }
}