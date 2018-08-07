﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using smartHookah.Helpers;
using smartHookah.Hubs;
using smartHookah.Models;
using smartHookah.Support;
using smartHookahCommon;

namespace smartHookah.Controllers
{
    public class QRCodeController : ApiController
    {
        private static IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();
        private readonly IRedisService _redisService;
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

                    Task.Factory.StartNew(() => IFFTHelper.PushToMakerConnect(id)
                    );

                }
                catch (Exception)
                {
                    
                    throw;
                }

            var sessionId = _redisService.GetSmokeSessionId(id);
            //var request = GetBaseUrl();
            var request = "http://app.manapipes.com/";
            var url = request + "smoke/"+ sessionId;
            
            var result = QrCodeHelper.GetBase64QrCode(url);

            if (hookah.Version < 1000003)
                {
                    qrResult = result;
                }
            else
            {
                 
                var initString = DeviceControlController.GetDeviceInitString(id,hookah.Version, db, _redisService);
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
