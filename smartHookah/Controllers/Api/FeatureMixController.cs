using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using smartHookah.ErrorHandler;
using smartHookah.Models.Dto;
using smartHookah.Services.FeatureMix;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/FeatureMix")]
    [ApiAuthorize]
    public class FeatureMixController : ApiController
    {
        private readonly IFeatureMixService featureMixService;

        public FeatureMixController(IFeatureMixService featureMixService)
        {
            this.featureMixService = featureMixService;
        }


        [Route("FeatureCreators")]
        public List<FeatureMixCreatorDto> GetFeatureMixCreators()
        {
            return FeatureMixCreatorDto.FromModelList(this.featureMixService.GetFeatureMixCreators()).ToList();
        }

        [Route("FeatureCreator/{id}")]
        public FeatureMixCreatorDto GetFeatureMixCreator(int id)
        {
            return FeatureMixCreatorDto.FromModel(this.featureMixService.GetFeatureMixCreator(id));
        }

        [Route("FollowedCreators")]
        public List<FeatureMixCreatorDto> GetFollowedCreators()
        {
            return FeatureMixCreatorDto.FromModelList(this.featureMixService.GetFollowedMixCreators()).ToList();
        }

        [Route("Follow/{id}")]
        [HttpPost]
        public void AddFollow(int id)
        {
          this.featureMixService.AddFollow(id);
        }

        [Route("Follow/{id}")]
        [HttpDelete]
        public void RemoveFollow(int id)
        {
            this.featureMixService.RemoveFollow(id);
        }

        [Route("Fix")]
        [HttpPost]
        [ApiAuthorize(Roles = "Admin")]
        public void Fix()
        {
            this.featureMixService.CreateFeatureMixCreatorFromOld();
        }
    }
}