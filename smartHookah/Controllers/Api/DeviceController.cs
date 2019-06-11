using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.ErrorHandler;
using smartHookah.Models.Db;
using smartHookah.Models.Dto.Device;
using smartHookah.Models.ParameterObjects;
using smartHookah.Services.Device;
using smartHookah.Services.Person;

namespace smartHookah.Controllers.Api
{
    using smartHookah.Models.Dto;

    [ApiAuthorize]
    [RoutePrefix("api/Device")]
    public class DeviceController : ApiController
    {
        private readonly IDeviceService deviceService;

        private readonly IDeviceSettingsPresetService deviceSettingsPresetService;

        private readonly IPersonService personService;

        private readonly IUpdateService updateService;

        private readonly IDevicePictureService devicePictureService;

        public DeviceController(IDeviceService deviceService, IDeviceSettingsPresetService deviceSettingsPresetService, IUpdateService updateService, IDevicePictureService devicePictureService)
        {
            this.deviceService = deviceService;
            this.deviceSettingsPresetService = deviceSettingsPresetService;
            this.updateService = updateService;
            this.devicePictureService = devicePictureService;
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
            var added = await this.personService.AddDevice(id);
            return DeviceSimpleDto.FromModel(added);
        }

        [HttpDelete, Route("{id}/Remove")]
        public async Task<DeviceSimpleDto> RemoveDevice(string id)
        {
            var added = await this.personService.RemoveDevice(id);
            return DeviceSimpleDto.FromModel(added);
        }

        [HttpPost, Route("{id}/Update/{updateId}")]
        public async Task<bool> PromptUpdate(int id, int updateId)
        {
           return await this.deviceService.UpdateDevice(id, updateId, this.personService.GetCurentPerson(), User.IsInRole("Admin"));
        }

        [HttpGet, Route("Updates")]
        public async Task<ICollection<UpdateDto>> Updates()
        {
            var updates = await this.updateService.GetUpdates();
            return updates.Select(UpdateDto.FromModel).ToList();
        }

        [HttpGet, Route("{id}/Info")]
        public async Task<DeviceInfoResponse> Info(int id)
        {
            var picture = await devicePictureService.FindStandPicture(id);

            var response = new DeviceInfoResponse {Picture = DevicePictureDto.FromModel(picture)};

            return response;
        }

        [HttpPost, Route("{id}/SetPicture")]
        public async Task<bool> SetPicture(int id, [FromBody]int pictureId)
        {
             return await devicePictureService.SetStandPicture(id,pictureId);
        }

        [HttpGet, Route("Pictures")]
        public async Task<ICollection<DevicePictureDto>> GetPictures()
        {
            var pictures = await devicePictureService.GetAllPictures(null);

            return pictures.Select(DevicePictureDto.FromModel).ToList();
        }


    }

    public class DeviceInfoResponse
    {
        public DevicePictureDto Picture { get; set; }

    }
}