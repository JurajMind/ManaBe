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

        [HttpGet, Route("{id}/GetPreset")]
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

        [HttpGet, Route("GetUserPresets")]
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
        public DevicePresetDto SaveSessionPreset(
            [FromUri] string sessionCode,
            string name,
            bool addToPerson = true,
            bool setDefault = true)
        {
            if (string.IsNullOrEmpty(sessionCode))
            {
                return null;
            }

            try
            {
                if (!addToPerson)
                {
                    if (!this.User.IsInRole("Admin"))
                    {
                        var err = new HttpError("Only admin can add global presets.");
                        throw new HttpResponseException(
                            this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, err));
                    }
                }
                var preset = this.deviceSettingsPresetService.SaveSessionPreset(sessionCode, name, addToPerson);
                if (addToPerson && setDefault)
                {
                    this.deviceSettingsPresetService.SetDefault(preset.Id);
                }

                return DevicePresetDto.FromModel(preset);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, err));
            }
        }

        [HttpPost, Route("SavePresetFromDevice/{deviceId}")]
        public DevicePresetDto SaveDevicePreset([FromUri] string deviceId, string name, bool addToPerson = true, bool setDefault = true)
        {
            try
            {
                var preset = this.deviceSettingsPresetService.SaveDevicePreset(deviceId, name, addToPerson);
                if (preset == null)
                {
                    throw new KeyNotFoundException("Device not found.");
                }
                if (addToPerson && setDefault)
                {
                    this.deviceSettingsPresetService.SetDefault(preset.Id);
                }

                return DevicePresetDto.FromModel(preset);
            }
            catch (KeyNotFoundException e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, err));
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
            await this.deviceSettingsPresetService.UseDefaut(sessionId);
        }

        [HttpPost, Route("{presetId}/Use/{sessionId}")]
        public async Task UsePreset(string sessionId, int presetId)
        {
            await this.deviceSettingsPresetService.UsePreset(sessionId, presetId);
        }

        [HttpDelete, Route("{id}/Delete")]
        public async Task<HttpResponseMessage> DeletePreset(int id)
        {
            try
            {
                await this.deviceSettingsPresetService.Delete(id);
                return this.Request.CreateResponse(HttpStatusCode.OK, $"Item {id} deleted.");
            }
            catch (Exception e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}