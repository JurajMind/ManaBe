using System.Linq;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;
using smartHookah.Services.Device;
using smartHookah.Services.Person;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public class SmokeViewMapper : ISmokeViewMapper
    {
        private readonly SmartHookahContext db;
        private readonly IIotService iotService;
        private readonly IPersonService personService;
        private readonly IMetadataModalViewModelMapper metadataModalViewModelMapper;

        public SmokeViewMapper(SmartHookahContext db, IIotService iotService, IPersonService personService, IMetadataModalViewModelMapper metadataModalViewModelMapper)
        {
            this.db = db;
            this.iotService = iotService;
            this.personService = personService;
            this.metadataModalViewModelMapper = metadataModalViewModelMapper;
        }

        public async Task<SmokeViewModel> Map(string sessionId)
        {
            var result = new SmokeViewModel();
            var session = db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);

            result.Hookah = db.Hookahs.Find(session.Hookah.Id);
            result.StandSetting = DeviceControlController.GetDeviceSettingViewModel(
                result.Hookah.Setting,
                result.Hookah.Version,
                db);
            result.State = await this.iotService.GetOnlineState(session.Hookah.Code);
            result.SessionId = session.SessionId;
            result.StandSetting.SessionId = result.SessionId;
            Models.Db.Person person = null;
            try
            {
                person = this.personService.GetCurentPerson();
            }
            catch (System.Exception)
            {
            }

            result.SmokeMetadataModalModal = this.metadataModalViewModelMapper.Map(sessionId,result.MetaData, person, out var outMetaData);

            result.MetaData = outMetaData;
            result.ShareToken = session.Token;
            result.Session = session;

            if (person != null) result.IsAssigned = session.IsPersonAssign(person.Id);
            result.SessionReview = db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == session.Id);

            if (result.SessionReview == null)
            {
                result.SessionReview = new TobaccoReview();
                result.SessionReview.SmokeSessionId = session.Id;
            }

            result.CurentState = PufType.Idle;

            return result;
        }
    }
}