using smartHookah.Models.Db;

namespace smartHookah.Controllers.Api
{
    using smartHookah.Models.Dto;
    using smartHookah.Services.Device;
    using System.Web.Http;

    [System.Web.Http.RoutePrefix("api/Animations")]
    public class AnimationController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IDeviceService deviceService;

        public AnimationController(SmartHookahContext db, IDeviceService deviceService)
        {
            this.db = db;
            this.deviceService = deviceService;
        }
        #region Getters

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetAnimations")]
        public AnimationsDTO GetAnimations(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new AnimationsDTO()
                {
                    Success = false,
                    Message = $"Id \'{id}\' not valid.",
                    HttpResponseCode = 404
                };

            var version = this.deviceService.GetDeviceVersion(id);
            if (version < 0) return new AnimationsDTO()
            {
                Success = false,
                Message = $"No data found for id \'{id}\'.",
                HttpResponseCode = 404
            };

            return new AnimationsDTO(this.deviceService.GetAnimations(), version)
            {
                Success = true,
                Message = "Animations loaded.",
                HttpResponseCode = 200
            };
        }

        #endregion
    }
}
