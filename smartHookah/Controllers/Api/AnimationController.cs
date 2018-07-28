using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Services.Device;

namespace smartHookah.Controllers
{
    [System.Web.Http.RoutePrefix("api/Animations")]
    public class AnimationController : ApiController
    {
        private readonly SmartHookahContext _db;

        public AnimationController(SmartHookahContext db)
        {
            this._db = db;
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
            var service = new DeviceService(_db, null);
            var version = service.GetDeviceVersion(id);
            if (version < 0) return new AnimationsDTO()
            {
                Success = false,
                Message = $"No data found for id \'{id}\'.",
                HttpResponseCode = 404
            };

            return new AnimationsDTO(service.GetAnimations(), version)
            {
                Success = true,
                Message = "Animations loaded.",
                HttpResponseCode = 200
            };
        }

        [System.Web.Http.ActionName("DefaultAction")]
        public HttpResponseMessage GetAnimation(string id)
        {
            var result = string.Join(" ", Enumerable.Repeat(id, 10));
            result += '\r';
            string yourJson = result;
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(yourJson, Encoding.UTF8, "application/json");
            return response;
        }

        #endregion
    }
}
