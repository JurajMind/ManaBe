using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.TeamFoundation.VersionControl.Client;

using smartHookah.Models;
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
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.SetBrightness(id, model.Brightness, model.Type);
            }
            catch (ItemNotFoundException e)
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
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.SetSpeed(id, model.Speed, model.Type);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.Sleep(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.Restart(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.SetMode(id, mode);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
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
                await this.deviceService.ShowQrCode(id);
            }
            catch (ItemNotFoundException e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Route("{id}/GetSetting")]
        public StandSettings GetSetting(string id)
        {
            try
            {
                var setting = this.deviceService.GetStandSettings(id);
                return StandSettings.FromModel(setting);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }

        #region Device preset settings

        #region Getters

        [HttpGet, Route("Preset/{id}/GetPreset")]
        public async Task<DevicePresetDto> GetPreset(int id)
        {
            try
            {
                var preset = await this.deviceSettingsPresetService.GetPreset(id);
                return DevicePresetDto.FromModel(preset);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, err));
            }
        }

        [HttpGet, Route("Preset/GetUserPresets")]
        public IEnumerable<DevicePresetDto> GetUserPresets()
        {
            try
            {
                var presets = this.deviceSettingsPresetService.GetUserPresets();
                return DevicePresetDto.FromModelList(presets);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }

        #endregion

        #region Setters

        [HttpPost, Route("Preset/{sessionCode}/SavePresetFromSession")]
        public int SavePreset([FromUri] string sessionCode, string name, bool addToPerson = true, bool setDefault = true)
        {
            if (string.IsNullOrEmpty(sessionCode))
            {
                return -1;
            }

            try
            {
                var presetId = this.deviceSettingsPresetService.SavePreset(sessionCode, name, addToPerson);
                if (addToPerson && setDefault)
                {
                    deviceSettingsPresetService.SetDefault(presetId);
                }
                return presetId;
            }
            catch (ItemNotFoundException e)
            {
                return -1;
            }
        }

        [HttpPost, Route("Preset/{deviceId}/SavePresetFromDevice")]
        public int SavePreset([FromUri] int deviceId, string name, bool addToPerson = true, bool setDefault = true)
        {
            try
            {
                var presetId = this.deviceSettingsPresetService.SavePreset(deviceId, name, addToPerson);
                if (addToPerson && setDefault)
                {
                    deviceSettingsPresetService.SetDefault(presetId);
                }
                return presetId;
            }
            catch (ItemNotFoundException e)
            {
                return -1;
            }
        }
        
        [HttpPost, Route("Preset/{id}/SetDefault")]
        public void SetDefault(int id)
        {
            this.deviceSettingsPresetService.SetDefault(id);
        }




        [HttpPost, Route("Preset/{id}/UseDefault")]
        public async Task UseDefault(string id)
        {
            var result = await this.deviceSettingsPresetService.UseDefaut(id);
        }


        [HttpPost, Route("Preset/{id}/Use/{presetId}")]
        public async Task UsePreset(string id,int presetId)
        {
            var result = await this.deviceSettingsPresetService.UsePreset(id, presetId);
        }

        #endregion

        [HttpDelete, Route("Preset/{id}/Delete")]
        public HttpResponseMessage DeletePreset(int id)
        {
            try
            {
                this.deviceSettingsPresetService.Delete(id);
                return this.Request.CreateResponse(this.Request.CreateErrorResponse(HttpStatusCode.OK, $"Item {id} deleted."));
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }

        #endregion
    }
}
