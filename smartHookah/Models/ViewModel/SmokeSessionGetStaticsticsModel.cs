using System.Collections.Generic;
using System.Linq;
using smartHookah.Helpers;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class SmokeSessionGetStaticsticsModel
    {

        public SmokeSessionGetStaticsticsModel Create(SmartHookahContext db,int id)
        {
        
            this.SmokeSession = db.SmokeSessions.Find(id);
       

            this.SessionReview = db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == this.SmokeSession.Id);

            if (SessionReview == null)
            {
                this.SessionReview = new TobaccoReview();
            }
            SessionReview.SmokeSessionId = SmokeSession.Id;
            SmokeSessionMetaData outMetaData;
            this.SmokeMetadataModalViewModel = SmokeMetadataModalViewModel.CreateSmokeMetadataModalViewModel(db,this.SmokeSession.SessionId,
               UserHelper.GetCurentPerson(db), out outMetaData);
            //var pufs = model.SmokeSession.Pufs.Select(a => (Puf) a).ToList();
            var pufs =
                db.DbPufs.Where(p => p.SmokeSession_Id == this.SmokeSession.Id).ToList().Select(a => (Puf)a).OrderBy(a => a.DateTime).ToList();
            this.LiveStatistic = SmokeHelper.GetSmokeStatistics(pufs);
            this.Histogram = SmokeHelper.CreateHistogram(pufs, 300);
            var user = UserHelper.GetCurentPerson(db);
            if (user != null)
                this.IsAssigned = SmokeSession.IsPersonAssign(user.Id);
            else
            {
                Share = true;
            }
            return this;
        }
        public SmokeStatisticViewModel LiveStatistic { get; set; }
        public SmokeSession SmokeSession { get; set; }
        public TobaccoReview SessionReview { get; set; }
        public SmokeMetadataModalViewModel SmokeMetadataModalViewModel { get; set; }
        public List<List<Puf>> Histogram { get; set; }
        public bool Share { get; set; }

        public bool IsAssigned { get; set; }
        public bool Public { get; set; }
        

    }
}