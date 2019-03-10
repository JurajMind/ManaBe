
using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using Newtonsoft.Json.Linq;

    using smartHookah.Helpers;
    using smartHookah.Models;
    using smartHookah.Services.Device;
    using smartHookah.Services.Person;

    using smartHookahCommon;

    public class DeviceControlController : Controller
    {
        private readonly SmartHookahContext db;

        private readonly IDeviceService deviceService;

        private readonly IDeviceSettingsPresetService devicePresetService;

        public DeviceControlController(SmartHookahContext db, IDeviceService deviceService, IDeviceSettingsPresetService devicePresetService)
        {
            this.db = db;
            this.deviceService = deviceService;
            this.devicePresetService = devicePresetService;
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
                deviceId = RedisHelper.GetHookahId(id);
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

            if (hookah.Version == 0)
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"spd:{dataChunk[0]}{dataChunk[1]}");
            }
            else
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"spd:{speedIndex},{speedValue},");
            }

            var setting = hookah.Setting;


            if (setting == null)
            {
                setting = new DeviceSetting();
                hookah.Setting = setting;
                this.db.Hookahs.AddOrUpdate(hookah);
            }

            setting.SetSpeed(speedIndex, speedValue);

            this.db.HookahSettings.AddOrUpdate(setting);
            await this.db.SaveChangesAsync();

            return new JsonResult();
        }

        private async Task ChangeColor(string data, string deviceId)
        {
            JObject jObject = JObject.Parse(data);

            var hookah = this.db.Hookahs.First(a => a.Code == deviceId);

            if (hookah.Version < 1000002)
            {
                var red = jObject["r"].Value<int>().ToString("000");
                var green = jObject["g"].Value<int>().ToString("000");
                var blue = jObject["b"].Value<int>().ToString("000");
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"clr:{red},{green},{blue}");
            }

            if (hookah.Version >= 1000002)
            {
                var h = (int) (jObject["h"].Value<double>()*255);
                var s = (int) (jObject["s"].Value<double>()*255);
                var v = (int) (jObject["v"].Value<double>()*255);
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"clr:{h:000},{s:000},{v:000}");
                hookah.Setting.Color.Hue = (byte) h;
                hookah.Setting.Color.Saturation = (byte) s;
                hookah.Setting.Color.Value = (byte) v;

                this.db.HookahSettings.AddOrUpdate(hookah.Setting);
                await this.db.SaveChangesAsync();
            }


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

            if (hookah.Version == 0)
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"led:{dataChunk[0]}{dataChunk[1]}");
            }
            else
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"led:{AnimationIndex},{AnimationValue},");
            }

            var setting = hookah.Setting;


            if (setting == null)
            {
                setting = new DeviceSetting();
                hookah.Setting = setting;
                this.db.Hookahs.AddOrUpdate(hookah);
            }


            setting.SetAnimation(AnimationIndex, AnimationValue);

            this.db.HookahSettings.AddOrUpdate(setting);
            await this.db.SaveChangesAsync();

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

            if (hookah.Version == 0)
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"br:{dataChunk[0]}{dataChunk[1]}");
            }
            else
            {
                await IotDeviceHelper.SendMsgToDevice(deviceId, $"br:{brIndex},{brValue},");
            }

            var setting = hookah.Setting;


            if (setting == null)
            {
                setting = new DeviceSetting();
                hookah.Setting = setting;
                this.db.Hookahs.AddOrUpdate(hookah);
            }
            
            setting.SetBrightness(brIndex, brValue);

            this.db.HookahSettings.AddOrUpdate(setting);
            await this.db.SaveChangesAsync();

            return new JsonResult();
        }

        

        public enum Animation
        {
            Off = 0,
            SmokeBar = 1,
            Flicker = 2,
            OneColor = 3,
            Rainbow = 4,
            PresureBar = 5,
            PresureBreath = 6,
            SelectedColor = 7,
            RainbowFade = 8,
            RainbowLoop = 9,
            RandomBurst = 10,
            ColorBounce = 11,
            ColorBounceFade = 12,
            EmsLightOne = 13,
            EmsLightAll = 14,
            Flicker2 = 15,
            Breath = 16,
            BreathInverse = 17,
            FadeVertical = 18,
            Rule30 = 19,
            RandomMarch = 20,
            RwbMarch = 21,
            Radiation = 22,
            ColorLoop = 23,
            Pop = 24,
            PresureColor = 25,
            Flame = 26,
            RainbowVertica = 27,
            Pacman = 28,
            RandomColorPop = 29,
            EmsStrobo = 30,
            RgbPropeller = 31,
            Kitt = 32,
            Matrix = 33,
            NewRainbow = 34,
        }

        public async Task<ActionResult> GetDeviceSetting(string id)
        {
            var hookahId = RedisHelper.GetHookahId(id);


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



        public static async Task InitDevice(string id, string version)
        {
            var sessionId = RedisHelper.GetSmokeSessionId(id);

            var pufs = RedisHelper.GetPufs(sessionId);

            var intake = pufs.Count(a => a.Type == PufType.In);

            var setting = new DeviceSetting();
            var versionInt = Helper.UpdateVersionToInt(version);
            using (var db = new SmartHookahContext())
            {
                var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);

                if (hookah?.Setting != null)
                    setting = hookah.Setting;
                var msg = string.Empty;
                if (versionInt <= 1000002)
                    msg = $"init:{setting.GetInitString()}{intake},150";
                else
                {
                    msg = $"init:{setting.GetInitStringWithColor(intake)}";
                }

                if(versionInt < 1000003)
                await IotDeviceHelper.SendMsgToDevice(id, msg);

                if (hookah.Version != versionInt)
                {
                    hookah.Version = versionInt;
                    db.Hookahs.AddOrUpdate(hookah);
                    await db.SaveChangesAsync();
                }

            }
        }

        public static string GetDeviceInitString(string id, int hookahVersion, SmartHookahContext context)
        {
            var sessionId = RedisHelper.GetSmokeSessionId(id);

            var pufs = RedisHelper.GetPufs(sessionId);

            var intake = pufs.Count(a => a.Type == PufType.In);
            var setting = new DeviceSetting();

            var hookah = context.Hookahs.FirstOrDefault(a => a.Code == id);

            if (hookah?.Setting != null)
                setting = hookah.Setting;

            var percentage = 300;
            var dbSession = context.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);

            if (dbSession != null && dbSession.MetaData != null && dbSession.MetaData.Tobacco != null &&
                dbSession.MetaData.Tobacco.Statistics != null)
            {
                percentage = (int) dbSession.MetaData.Tobacco.Statistics.PufCount;
            }

            if(hookahVersion < 1000011)
            return setting.GetInitStringWithColor(intake);

            if(hookahVersion < 1000017)
            return setting.GetInitStringWithPercentage(intake, percentage);

            if(hookahVersion < 1000024)
            return setting.GetInitStringWithBrightness(intake, percentage);

            if (hookahVersion < 1000025)
                return setting.GetInitStringWithSessionId(intake, percentage, sessionId);

            return setting.GetInitStringWithSpeed(intake, percentage, sessionId);
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
