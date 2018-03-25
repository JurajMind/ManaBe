using System.Linq;
using System.Threading.Tasks;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    public class SmokeViewModel
    {

        public async Task<SmokeViewModel> CreateModel(SmartHookahContext db,int? dbSmokeSessionId = null, string redisSessionId = null,string reddisHookahId = null)
        {
           
            var session = new SmokeSession();


            if (!string.IsNullOrEmpty(redisSessionId))
                session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == redisSessionId);


            if (!string.IsNullOrEmpty(reddisHookahId))
            {
                var rs = RedisHelper.GetSmokeSessionId(reddisHookahId);
                redisSessionId = rs;
                session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == rs);
            }
               

            if (dbSmokeSessionId.HasValue)
             session = db.SmokeSessions.Find(dbSmokeSessionId);

            if (session == null)
            {
                session =  SmokeSessionController.InitSmokeSession(db,reddisHookahId,redisSessionId);
            }


            this.Hookah = db.Hookahs.Find(session.Hookah.Id);
            this.StandSetting = DeviceControlController.GetDeviceSettingViewModel(this.Hookah.Setting, this.Hookah.Version,db);
            this.State = await IotDeviceHelper.GetState(session.Hookah.Code);
            this.SessionId = session.SessionId;
            this.StandSetting.SessionId = SessionId;
            SmokeSessionMetaData outMetaData;
            this.SmokeMetadataModalModal = SmokeMetadataModalViewModel.CreateSmokeMetadataModalViewModel(db,this.SessionId, UserHelper.GetCurentPerson(db),out outMetaData);
            this.MetaData = outMetaData;
            this.ShareToken = session.Token;
            this.Session = session;
            var user = UserHelper.GetCurentPerson(db);
            if(user != null)
                this.IsAssigned = session.IsPersonAssign(user.Id);
            this.SessionReview = db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == session.Id);

            if (this.SessionReview == null)
            {
                this.SessionReview = new TobaccoReview();
                this.SessionReview.SmokeSessionId = session.Id;
            }

            var lastState = RedisHelper.GetPufs(this.SessionId).LastOrDefault();
          
               this.CurentState = PufType.Idle;

            return this;
            
        }

        public SmokeViewModel() { }
        public Hookah Hookah { get; set; }
        public SmokeSession Session { get; set; }
        public string SessionId
        { get; set; }

        public bool CanEndSession { get; set; }

        public PufType CurentState { get; set; }

        public bool State { get; set; }

        public SmokeMetadataModalViewModel SmokeMetadataModalModal { get; set; }

        public SmokeSessionMetaData MetaData { get; set; }

        public DeviceControlController.DeviceSettingViewModel StandSetting { get; set; }
        public TobaccoReview SessionReview { get; set; } = new TobaccoReview();
        public string ShareToken { get; set; }
        public bool Share { get; set; }

        public bool IsAssigned { get; set; }
    }

    public class MetadataAndReviewModel
    {
        public TobaccoReview Review { get; set; }

        public SmokeSessionMetaData MetaData { get; set; }
    }
}