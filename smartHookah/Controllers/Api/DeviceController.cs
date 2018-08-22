using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Models;
using smartHookah.Models.ParameterObjects;
using smartHookah.Services.Device;

namespace smartHookah.Controllers.Api
{
    using MaxMind.GeoIP2.Exceptions;

    using smartHookah.Models.Dto;

    [RoutePrefix("api/Device")]
    public class DeviceController : ApiController
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost, Route("{id}/ChangeAnimation")]
        public async Task<HttpResponseMessage> ChangeAnimation(string id, [FromBody] ChangeAnimation model)
        {
            if (string.IsNullOrEmpty(id) || model.AnimationId < 0 || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                var animation = _deviceService.GetAnimation(model.AnimationId);
                await _deviceService.SetAnimation(id, animation, model.Type);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeBrightness")]
        public async Task<HttpResponseMessage> ChangeBrightness(string id, [FromBody] ChangeBrightness model)
        {
            if (string.IsNullOrEmpty(id) || model.Brightness < 0 || model.Brightness > 255 || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.SetBrightness(id, model.Brightness, model.Type);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeColor")]
        public async Task<HttpResponseMessage> ChangeColor(string id, [FromBody] ChangeColor model)
        {
            if (string.IsNullOrEmpty(id) || model.Color == null || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.SetColor(id, model.Color, model.Type);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeSpeed")]
        public async Task<HttpResponseMessage> ChangeSpeed(string id, [FromBody] ChangeSpeed model)
        {
            if (string.IsNullOrEmpty(id) || model.Speed < 0 || model.Speed > 255 || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.SetSpeed(id, model.Speed, model.Type);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/Sleep")]
        public async Task<HttpResponseMessage> Sleep(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.Sleep(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/Restart")]
        public async Task<HttpResponseMessage> Restart(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.Restart(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeMode")]
        public async Task<HttpResponseMessage> ChangeMode(string id, [FromBody] int mode)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.SetMode(id, mode);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ShowQrCode")]
        public async Task<HttpResponseMessage> ShowQrCode(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await _deviceService.ShowQrCode(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        [HttpGet, Route("{id}/StandSetting")]
        public StandSettings GetStandSetting(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Stand with id {id} not found"));
            }

            var setting = this._deviceService.GetStandSettings(id);

            if (setting == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Stand with id {id} not found"));
            }

            return StandSettings.FromModel(setting);

        }
    }
}
