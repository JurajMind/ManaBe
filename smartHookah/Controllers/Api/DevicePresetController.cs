using System;

namespace smartHookah.Controllers.Api
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.TeamFoundation.VersionControl.Client;

    using smartHookah.Models.Dto;
    using smartHookah.Services.Person;

    [RoutePrefix("api/Device/Preset")]
    public class DevicePresetController : ApiController
    {
        private readonly IDeviceSettingsPresetService deviceSettingsPresetService;

        public DevicePresetController(IDeviceSettingsPresetService deviceSettingsPresetService)
        {
            this.deviceSettingsPresetService = deviceSettingsPresetService;
        }

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
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, err));
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

        [HttpPost, Route("{sessionCode}/SavePresetFromSession")]
        public int SaveSessionPreset(
            [FromUri] string sessionCode,
            string name,
            bool addToPerson = true,
            bool setDefault = true)
        {
            if (string.IsNullOrEmpty(sessionCode))
            {
                return -1;
            }

            try
            {
                if (!addToPerson)
                {
                    if (User.IsInRole("Admin"))
                    {
                        var err = new HttpError("Only admin can add default preset");
                        throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.Forbidden,err));
                    }
                        
                }
                var presetId = this.deviceSettingsPresetService.SaveSessionPreset(sessionCode, name, addToPerson);
                if (addToPerson && setDefault)
                {
                    this.deviceSettingsPresetService.SetDefault(presetId);
                }

                return presetId;
            }
            catch (ItemNotFoundException e)
            {
                return -1;
            }
        }

        [HttpPost, Route("SavePresetFromDevice/{deviceId}")]
        public int SaveDevicePreset([FromUri] string deviceId, string name, bool addToPerson = true, bool setDefault = true)
        {
            try
            {
                var presetId = this.deviceSettingsPresetService.SaveDevicePreset(deviceId, name, addToPerson);
                if (addToPerson && setDefault)
                {
                    this.deviceSettingsPresetService.SetDefault(presetId);
                }

                return presetId;
            }
            catch (ItemNotFoundException e)
            {
                return -1;
            }
        }

        [HttpPost, Route("{presetId}/SetDefault")]
        public void SetDefault(int presetId)
        {
            this.deviceSettingsPresetService.SetDefault(presetId);
        }

        [HttpPost, Route("UseDefault/{sessionId}")]
        public async Task UseDefault(string sessionId)
        {
            var result = await this.deviceSettingsPresetService.UseDefaut(sessionId);
        }

        [HttpPost, Route("{presetId}/Use/{sessionId}")]
        public async Task UsePreset(string sessionId, int presetId)
        {
            var result = await this.deviceSettingsPresetService.UsePreset(sessionId, presetId);
        }

        [HttpDelete, Route("{id}/Delete")]
        public HttpResponseMessage DeletePreset(int id)
        {
            try
            {
                this.deviceSettingsPresetService.Delete(id);
                return this.Request.CreateResponse(
                    this.Request.CreateErrorResponse(HttpStatusCode.OK, $"Item {id} deleted."));
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                return this.Request.CreateResponse(this.Request.CreateErrorResponse(HttpStatusCode.NotFound, err));
            }
        }
    }
}