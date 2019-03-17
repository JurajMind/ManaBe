
using smartHookah.Mappers.ViewModelMappers.Smoke;
using smartHookah.Models.Db;
using smartHookah.Services.Redis;

namespace smartHookah.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Newtonsoft.Json.Linq;

    using Helpers;
    using smartHookah.Services.Device;
    using smartHookah.Services.Person;

    using smartHookahCommon;

    public class DeviceControlController : Controller
    {
        private readonly SmartHookahContext db;

        private readonly IDeviceService deviceService;

        private readonly IDeviceSettingsPresetService devicePresetService;

        private readonly IRedisService redisService;

        private readonly IMetadataModalViewModelMapper metadataModalMapper;

        public DeviceControlController(SmartHookahContext db, IDeviceService deviceService, IDeviceSettingsPresetService devicePresetService, IRedisService redisService, IMetadataModalViewModelMapper metadataModalMapper)
        {
            this.db = db;
            this.deviceService = deviceService;
            this.devicePresetService = devicePresetService;
            this.redisService = redisService;
            this.metadataModalMapper = metadataModalMapper;
        }

        public async Task<ActionResult> DefaultMetadata(int? hookahId, int?personId)
        {
            var model = new DefaultMetadataViewModel { hookahId = hookahId, personId = personId };
            SmokeSessionMetaData metadata = null;
            if (hookahId != null)
            {
                var hookah = this.db.Hookahs.FirstOrDefault(a => a.Id == hookahId);
                if (hookah != null)
                {
                    metadata = hookah.DefaultMetaData;
                }
                    
            }


            if (personId != null)
            {
                var person = this.db.Persons.FirstOrDefault(a => a.Id == personId);
                if (person != null)
                    metadata = person.DefaultMetaData;
            }

            if (metadata == null)
                metadata = new SmokeSessionMetaData();

            model.metadata = metadata;

            model.MetadataView =
                SmokeMetadataModalViewModel.CreateSmokeMetadataModalViewModel(
                    this.db, metadata,
                    UserHelper.GetCurentPerson(this.db));

            return this.View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveDefaultMetadata(int? hookahId, int?personId, SaveSmokeMetadataModel model)
        {
            var metadata = new SmokeSessionMetaData();
            SmokeSessionController.SetMetadata(model, metadata);

            if (personId != null)
            {
                var person = this.db.Persons.FirstOrDefault(a => a.Id == personId);
                if (person != null)
                {
                    person.DefaultMetaData = metadata;
                    this.db.Persons.AddOrUpdate(person);
                    await this.db.SaveChangesAsync();
                    return this.RedirectToAction("DefaultMetadata", new {personId = person.Id});
                }
            }

            if (hookahId != null)
            {
                var hookah = this.db.Hookahs.FirstOrDefault(a => a.Id == hookahId);
                if (hookah != null)
                {
                    hookah.DefaultMetaData = metadata;
                    this.db.Hookahs.AddOrUpdate(hookah);
                    await this.db.SaveChangesAsync();
                    return this.RedirectToAction("DefaultMetadata", new { hookahId = hookah.Id });
                }
            }

            return null;
        }

        // GET: DeviceControl
        public async Task<JsonResult> Sent(string id, string command, string data = null, string dvId = null)
        {

            if (!string.IsNullOrEmpty(id) && id.StartsWith("Place"))
            {
                var placeId = int.Parse(id.Remove(0, 5));

                var person = this.db.Persons.Where(a => a.Id == placeId).Include(a => a.Hookahs).FirstOrDefault();

                if(person == null)
                    return new JsonResult();

                foreach (var stand in person.Hookahs)
                {
                    await this.Sent(null, command, data, stand.Code);
                }
              

                return new JsonResult();
            }

            var deviceId = string.Empty;
            if (dvId != null)
            {
                deviceId = dvId;
            }
            else
            {
                deviceId = this.redisService.GetHookahId(id);
            }
       
            switch (command)
            {
                case "qrcode":
                    await IotDeviceHelper.SendMsgToDevice(deviceId, "qrcode:");
                    return new JsonResult();

                case "mode":
                    await IotDeviceHelper.SendMsgToDevice(deviceId, $"mode:{data[0]}");
                    return new JsonResult();

                case "led":
                    return await this.ChangeAnimaton(data, deviceId);

                case "br":
                {
                    return await this.ChangeBrightness(data, deviceId);
                }

                case "sleep":
                {
                    await IotDeviceHelper.SendMsgToDevice(deviceId, "slp:");
                        return new JsonResult();
                    }

                case "speed":
                {
                    return await this.ChangeSpeed(data, deviceId);
                    }
                    

                case "color":
                {
                    await this.ChangeColor(data, deviceId);
                        return new JsonResult();
                    }

                case "restart":
                    {
                        await IotDeviceHelper.SendMsgToDevice(deviceId, $"restart:");
                        return new JsonResult();
                    }

                    return new JsonResult();

            }
            return new JsonResult();
        }

        private async Task<JsonResult> ChangeSpeed(string data, string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null)
                return null;


            var dataChunk = data.Split(':');
            if (dataChunk.Length > 2)
                return new JsonResult();

            int speedIndex = 0;
            int speedValue = 0;


            if (!int.TryParse(dataChunk[0], out speedIndex) || !int.TryParse(dataChunk[1], out speedValue))
            {
                return new JsonResult();
            }

            await this.deviceService.SetSpeed(deviceId, speedValue,(PufType) speedIndex);

            return new JsonResult();
        }

        private async Task ChangeColor(string data, string deviceId)
        {
            JObject jObject = JObject.Parse(data);
            
            var h = (byte)(jObject["h"].Value<double>() * 255);
            var s = (byte)(jObject["s"].Value<double>() * 255);
            var v = (byte)(jObject["v"].Value<double>() * 255);
            await this.deviceService.SetColor(deviceId, new Color()
            {
                Hue = h,
                Saturation = s,
                Value = v,
            }, PufType.Idle);
        }

        private async Task<JsonResult> ChangeAnimaton(string data, string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null)
                return null;


            var dataChunk = data.Split(':');
            if (dataChunk.Length > 2)
                return new JsonResult();

            int AnimationIndex = 0;
            int AnimationValue = 0;

            if (!int.TryParse(dataChunk[0], out AnimationIndex) || !int.TryParse(dataChunk[1], out AnimationValue))
            {
                return new JsonResult();
            }

            var animation = this.deviceService.GetAnimation(AnimationValue);
            await this.deviceService.SetAnimation(deviceId, animation, (PufType) AnimationIndex);
            

            return new JsonResult();
        }

        private async Task<JsonResult> ChangeBrightness(string data, string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null)
                return null;


            var dataChunk = data.Split(':');
            if (dataChunk.Length > 2)
                return new JsonResult();

            int brIndex = 0;
            int brValue = 0;

            if (!int.TryParse(dataChunk[0], out brIndex) || !int.TryParse(dataChunk[1], out brValue))
            {
                return new JsonResult();
            }

            await this.deviceService.SetBrightness(deviceId, brValue, (PufType)brIndex);

            return new JsonResult();
        }


        public async Task<ActionResult> GetDeviceSetting(string id)
        {
            var hookahId = this.redisService.GetHookahId(id);


            var model = GetDeviceSettingViewModel(this.db, hookahId);

            return this.View(model);

        }

        public static DeviceSettingViewModel GetDeviceSettingViewModel(SmartHookahContext db, string hookahId,
            string sessionId = null)
        {
            var hookah = db.Hookahs.Where(b => b.Code == hookahId).Include(a => a.Setting).FirstOrDefault();
            var settings = hookah.Setting;

            return GetDeviceSettingViewModel(settings, hookah.Version);
        }

        public static DeviceSettingViewModel GetDeviceSettingViewModel(DeviceSetting setting , int? hookahVersion, SmartHookahContext db = null)
        {
            if (setting == null)
            { 
                setting = new DeviceSetting();
                setting.Color.Value = 255;
                setting.Color.Hue = 255;
                setting.Color.Saturation = 255;
                setting.PictureId = 1;
                

                if (db != null)
                {
                   var stored =  db.HookahSettings.Add(setting);
                    setting = stored;
                    db.SaveChanges();
                }
            }

            var model = new DeviceSettingViewModel();

            model.Setting = setting;
            if(hookahVersion.HasValue)
            model.HookahVersion = hookahVersion.Value;
            model.IdleAnimation = setting.IdleAnimation;
            model.PufAnimation =  setting.PufAnimation;
            model.BlowAnimation = setting.BlowAnimation;
            model.Animations = AnimationHelper.Animations;

            return model;
        }
        public class DeviceSettingViewModel
        {
            public int HookahVersion { get; set; }
            public string SessionId { get; set; }
            public DeviceSetting Setting { get; set; }
            public int IdleAnimation { get; set; }
            public int PufAnimation { get; set; }
            public int BlowAnimation { get; set; }

            public string SelectedColor
            {
                get
                {
                    return "#" + this.Setting.Color.Hue.ToString("X") + this.Setting.Color.Saturation.ToString("X") + this.Setting.Color.Value.ToString("X");
                }
            }

            public int ToInitColor
            {
                get { return int.Parse(this.SelectedColor, System.Globalization.NumberStyles.HexNumber); }
            }

            public List<Helpers.Animation> Animations { get; set; }

            
        }

        [HttpPost]
        public JsonResult SetDefault(int id)
        {
            this.devicePresetService.SetDefault(id);

            return this.Json(new { success = true });
        }



        [HttpPost]
        public JsonResult UseDefault(string id)
        {
           
            var result = this.devicePresetService.UseDefaut(id);
         
            return this.Json(new { success = result });

        }
    }

    public class DefaultMetadataViewModel 
    {
        public int? hookahId { get; set; }
        public int? personId { get; set; }

        public SmokeSessionMetaData metadata { get; set; }
        public SmokeMetadataModalViewModel MetadataView { get; set; }
    }
}
