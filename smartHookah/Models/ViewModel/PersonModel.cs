using smartHookah.Models.Db;
using smartHookah.Models.Redis;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<Reservation> ActiveReservations { get; set; }
    }

    public class ShowGearViewModel
    {
        public List<Tobacco> Tobaccos { get; set; }
        public List<Bowl> Bowls { get; set; }
        public List<Pipe> Pipes { get; set; }

        public List<Coal> Goals { get; set; }

        public List<HeatManagment> HeatManagments { get; set; }

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