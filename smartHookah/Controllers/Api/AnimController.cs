using smartHookah.Services.Device;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    public class AnimController : ApiController
    {
        private readonly IDeviceService deviceService;

        public AnimController(IDeviceService deviceService)
        {
            this.deviceService = deviceService;
        }

        [HttpPost]
        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public async Task<string> Update(string id)
        {
            string result = await Request.Content.ReadAsStringAsync();
            var bytes = Convert.FromBase64String(result);
            var str = System.Text.Encoding.Default.GetString(bytes);

            if (str.StartsWith("ani") && bytes.Length >= 9)
            {
                await this.deviceService.UpdateSettingFromDevice(id, bytes);
                return "OK";

            }

            return null;
        }
    }
}
