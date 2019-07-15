using System.Linq;
using System.Threading.Tasks;

using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    public class SmokeViewModel
    {
        public SmokeViewModel()
        {
        }

        public Hookah Hookah { get; set; }

        public SmokeSession Session { get; set; }

        public string SessionId { get; set; }

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