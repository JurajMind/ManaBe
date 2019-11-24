using Microsoft.AspNet.SignalR;
using smartHookah.Hubs;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;
using smartHookah.Support;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace smartHookah.Controllers
{
    using smartHookah.Services.Device;
    using smartHookah.Services.Messages;

    public class QRCodeController : ApiController
    {
        private static IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        private readonly IDeviceService deviceService;

        private readonly ISignalNotificationService _signalNotificationService;

        private readonly IRedisService redisService;

        public QRCodeController(IDeviceService deviceService, ISignalNotificationService signalNotificationService, IRedisService redisService)
        {
            this.deviceService = deviceService;
            this._signalNotificationService = signalNotificationService;
            this.redisService = redisService;
        }

        [HttpGet]
        [OptionalHttps(true)]
        [ActionName("DefaultAction")]
        public HttpResponseMessage GetQrCode(string id)
        {
            using (var db = new SmartHookahContext())
            {
                string qrResult = "";
                var hookah = db.Hookahs.FirstOrDefault(h => h.Code == id);
                if (hookah == null)
                    return null;

                try
                {
                    ClientContext.Clients.Group(hookah.Code).online(hookah.Code);
                    Task.Factory.StartNew(() => this._signalNotificationService.OnlineDevice(hookah.Code));


                }
                catch (Exception)
                {


                }

                var sessionId = redisService.GetSessionId(id);
                //var request = GetBaseUrl();
                var request = "http://app.manapipes.com/";
                var url = request + "smoke/" + sessionId;

                var result = QrCodeHelper.GetBase64QrCode(url);

                if (hookah.Version < 1000003)
                {
                    qrResult = result;
                }
                else
                {

                    var initString = this.deviceService.GetDeviceInitString(id, hookah.Version);
                    qrResult = $"{initString};{result}";
                }




                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(qrResult, Encoding.UTF8, "application/json");
                return response;
            }
        }

        private string GetBaseUrl()
        {
            var request = HttpContext.Current.Request;
            var appUrl = HttpRuntime.AppDomainAppVirtualPath;

            if (appUrl != "/")
                appUrl = "/" + appUrl;

            var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);

            return baseUrl;
        }
    }
}
