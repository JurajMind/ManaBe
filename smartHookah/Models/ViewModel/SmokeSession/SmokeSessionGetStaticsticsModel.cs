using smartHookah.Controllers;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Session;
using System.Collections.Generic;

namespace smartHookah.Models.ViewModel.SmokeSession
{
    public class SmokeSessionGetStaticsticsModel
    {
        public SmokeStatisticViewModel LiveStatistic { get; set; }
        public Db.SmokeSession SmokeSession { get; set; }
        public SessionReview SessionReview { get; set; }
        public SmokeMetadataModalViewModel SmokeMetadataModalViewModel { get; set; }
        public List<List<Puf>> Histogram { get; set; }
        public bool Share { get; set; }

        public bool IsAssigned { get; set; }
        public bool Public { get; set; }


    }
}