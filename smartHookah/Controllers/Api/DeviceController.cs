using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.Models.Db;
using smartHookah.Models.ParameterObjects;
using smartHookah.Services.Device;
using smartHookah.Services.Person;

namespace smartHookah.Controllers.Api
{
    using smartHookah.Models.Dto;

    [RoutePrefix("api/Device")]
    public class DeviceController : ApiController
    {
        private readonly IDeviceService deviceService;

        private readonly IDeviceSettingsPresetService deviceSettingsPresetService;

        public DeviceController(IDeviceService deviceService, IDeviceSettingsPresetService deviceSettingsPresetService)
        {
            this.deviceService = deviceService;
            this.deviceSettingsPresetService = deviceSettingsPresetService;
        }

        [HttpPost, Route("{id}/ChangeAnimation")]
        public async Task<HttpResponseMessage> ChangeAnimation(string id, [FromBody] ChangeAnimation model)
        {
            if (string.IsNullOrEmpty(id) || model.AnimationId < 0 || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                var animation = this.deviceService.GetAnimation(model.AnimationId);
                await this.deviceService.SetAnimation(id, animation, model.Type);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeBrightness")]
        public async Task<HttpResponseMessage> ChangeBrightness(string id, [FromBody] ChangeBrightness model)
        {
            if (string.IsNullOrEmpty(id) || model.Brightness < 0 || model.Brightness > 255
                || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.SetBrightness(id, model.Brightness, model.Type);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.SetColor(id, model.Color, model.Type);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeSpeed")]
        public async Task<HttpResponseMessage> ChangeSpeed(string id, [FromBody] ChangeSpeed model)
        {
            if (string.IsNullOrEmpty(id) || model.Speed < 0 || model.Speed > 600
                || !Enum.IsDefined(typeof(PufType), model.Type))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.SetSpeed(id, model.Speed, model.Type);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/Sleep")]
        public async Task<HttpResponseMessage> Sleep(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.Sleep(id);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/Restart")]
        public async Task<HttpResponseMessage> Restart(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.Restart(id);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ChangeMode")]
        public async Task<HttpResponseMessage> ChangeMode(string id, [FromBody] int mode)
        {
            if (string.IsNullOrEmpty(id)) return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.SetMode(id, mode);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost, Route("{id}/ShowQrCode")]
        public async Task<HttpResponseMessage> ShowQrCode(string id)
        {
            if (string.IsNullOrEmpty(id)) return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            try
            {
                await this.deviceService.ShowQrCode(id);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Route("{id}/GetSetting")]
        public DeviceSettingDto GetSetting(string id)
        {
            try
            {
                var setting = this.deviceService.GetStandSettings(id);
                return DeviceSettingDto.FromModel(setting);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }

        [HttpPost, Route("{id}/Add")]
        public async Task<DeviceSimpleDto> AddDevice(string id)
        {
            var added = await this.deviceService.AddDevice(id);
            return DeviceSimpleDto.FromModel(added);
        }

        [HttpDelete, Route("{id}/Remove")]
        public async Task<DeviceSimpleDto> RemoveDevice(string id)
        {
            var added = await this.deviceService.RemoveDevice(id);
            return DeviceSimpleDto.FromModel(added);
        }




    }
}