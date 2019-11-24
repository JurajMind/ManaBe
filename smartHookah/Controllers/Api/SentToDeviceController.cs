using smartHookah.Helpers;
using System.Web.Http;

namespace smartHookah.Controllers
{
    public class SentToDeviceController : ApiController
    {

        [System.Web.Http.ActionName("DefaultAction")]
        [System.Web.Http.AcceptVerbs("GET", "POST")]
        [System.Web.Http.HttpGet]
        public async void Sent(string id, string command, string data = null)
        {
            switch (command)
            {
                case "qrcode":
                    await IotDeviceHelper.SendMsgToDevice(id, "qrcode:");
                    return;

                case "mode":
                    await IotDeviceHelper.SendMsgToDevice(id, $"mode:{data[0]}");
                    return;

                case "animation":
                    await IotDeviceHelper.SendMsgToDevice(id, $"anim:{data[0]}");
                    return;

            }
        }
    }
}