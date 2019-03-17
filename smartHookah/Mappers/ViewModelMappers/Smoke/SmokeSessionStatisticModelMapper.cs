using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Controllers;
using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.ViewModel.SmokeSession;
using smartHookah.Services.Person;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public class SmokeSessionStatisticModelMapper : ISmokeSessionStatisticModelMapper
    {
        private readonly SmartHookahContext db;
        private readonly IMetadataModalViewModelMapper metadataModalViewModelMapper;
        private readonly IPersonService personService;

        public SmokeSessionStatisticModelMapper(SmartHookahContext db, IMetadataModalViewModelMapper metadataModalViewModelMapper, IPersonService personService)
        {
            this.db = db;
            this.metadataModalViewModelMapper = metadataModalViewModelMapper;
            this.personService = personService;
        }

        public SmokeSessionGetStaticsticsModel Map(int sessionId)
        {
            var result = new SmokeSessionGetStaticsticsModel();
            result.SmokeSession = db.SmokeSessions.Find(sessionId);


            result.SessionReview = db.TobaccoReviews.FirstOrDefault(a => a.SmokeSessionId == result.SmokeSession.Id);

            if (result.SessionReview == null)
            {
                result.SessionReview = new TobaccoReview();
            }
            result.SessionReview.SmokeSessionId = result.SmokeSession.Id;
            SmokeSessionMetaData outMetaData;
            result.SmokeMetadataModalViewModel = this.metadataModalViewModelMapper.Map(result.SmokeSession.SessionId,
                result.SmokeSession.MetaData, personService.GetCurentPerson(), out outMetaData);
         
            var pufs =
                result.SmokeSession.Pufs.ToList().Select(a => (Puf)a).OrderBy(a => a.DateTime).ToList();
            result.LiveStatistic = SmokeHelper.GetSmokeStatistics(pufs);
            result.Histogram = SmokeHelper.CreateHistogram(pufs, 300);
            var user = UserHelper.GetCurentPerson(db);
            if (user != null)
                result.IsAssigned = result.SmokeSession.IsPersonAssign(user.Id);
            else
            {
                result.Share = true;
            }
            return result;
        }
    }
}