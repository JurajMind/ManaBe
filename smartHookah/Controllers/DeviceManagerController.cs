using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Navigation;
using smartHookah.Models.Dto.Device;
using smartHookah.Services.Device;

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
