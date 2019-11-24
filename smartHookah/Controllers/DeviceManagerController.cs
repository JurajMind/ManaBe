using smartHookah.Models.Dto.Device;
using smartHookah.Services.Device;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Controllers
{
    [RoutePrefix("api/Device/Manage")]
    public class DeviceManagerController : ApiController
    {
        private readonly IDeviceManageService deviceManageService;

        public DeviceManagerController(IDeviceManageService deviceManageService)
        {
            this.deviceManageService = deviceManageService;
        }

        [HttpGet]
        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public async Task<DeviceCreationDto> CreateDevice()
        {
            return await this.deviceManageService.CreateDevice();
        }
    }
}
