using System;
using smartHookah.Controllers;
using smartHookah.Models.Db;
using smartHookah.Services.Messages;
using smartHookah.Services.Person;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;
    using smartHookah.Helpers;
    using smartHookah.Services.Redis;

    public class DeviceService : IDeviceService
    {
        private readonly SmartHookahContext db;

        private readonly IIotService iotService;

        private readonly IRedisService redisService;

        private readonly INotificationService notificationService;
        

        public DeviceService(SmartHookahContext db, IIotService iotService, IRedisService redisService, INotificationService notificationService)
        {
            this.db = db;
            this.iotService = iotService;
            this.redisService = redisService;
            this.notificationService = notificationService;
        }

        public async Task SetAnimation(string deviceId, Animation animation, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            if (animation.VersionFrom != 0 && animation.VersionTo != 0)
              if (animation.VersionFrom >= hookah.Version || animation.VersionTo <= hookah.Version)
                    throw new NotSupportedException($"Animation {animation.DisplayName} not supported by your Hookah OS version.");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"led:{(int)state},{animation.Id},");

            this.SetAnimation(hookah.Setting, (int)state, animation.Id);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this.notificationService.SessionSettingsChanged(deviceId, hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetBrightness(string deviceId, int brightness, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"br:{(int)state},{brightness},");

            this.SetBrightness(hookah.Setting, (int)state, brightness);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this.notificationService.SessionSettingsChanged(deviceId,hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        private Hookah getDevice(string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);
            if (hookah == null) return null;
            return hookah.Type == StandType.Emulator ? null : hookah;
        }

        public async Task SetSpeed(string deviceId, int speed, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"spd:{(int)state},{speed},");

            this.SetSpeed(hookah.Setting, (int)state, speed);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this.notificationService.SessionSettingsChanged(deviceId, hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetColor(string deviceId, Color color, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"clr:{color.Hue:000},{color.Saturation:000},{color.Value:000}");

            this.SetColor(hookah.Setting, (int)state, color);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this.notificationService.SessionSettingsChanged(deviceId, hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task Sleep(string deviceId)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "slp:");
        }

        public async Task Restart(string deviceId)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "restart:");
        }

        public async Task SetMode(string deviceId, int mode)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, $"mode:{mode}");
        }

        public async Task ShowQrCode(string deviceId)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "qrcode:");
        }

        public async Task SetPreset(string deviceId, int settingId)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            var setting = this.db.DevicePreset.FirstOrDefault(a => a.Id == settingId);

            if (setting == null) throw new KeyNotFoundException($"Person setting with id {settingId} not found");
            await this.SetPreset(deviceId, setting.DeviceSetting);
           
        }

        public async Task SetPreset(string deviceId, DeviceSetting setting)
        {
            var settingString = setting.GetInitStringWithSpeed(-1, -1, "-PR-");
            var device = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);
            if (device == null) return;

            device.Setting.Change(setting);
            this.db.HookahSettings.AddOrUpdate(device.Setting);
            await this.iotService.SendMsgToDevice(deviceId, $"pres:{settingString}");
        }

        public async Task UpdateSettingFromDevice(string deviceId, byte[] rawData)
        {
            int _idleAnimation = rawData[3];
            int _pufAnimation = rawData[4];
            int _blowAnimation = rawData[5];
            int _idleBr = rawData[6];
            int _pufBr = rawData[7];
            byte _hue = rawData[8];
            byte _sat = rawData[9];

          
                var hookah = db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

                var hookahSetting = hookah?.Setting;

                if (hookahSetting == null)
                    return;

                hookahSetting.IdleAnimation = _idleAnimation;
                hookahSetting.BlowAnimation = _blowAnimation;
                hookahSetting.PufAnimation = _pufAnimation;
                hookahSetting.IdleBrightness = _idleBr;
                hookahSetting.PufBrightness = _pufBr;
                hookahSetting.Color.Hue = _hue;
                hookahSetting.Color.Saturation = _sat;

                hookah.Setting = hookahSetting;
                db.Hookahs.AddOrUpdate(hookah);
                this.notificationService.SessionSettingsChanged(deviceId,hookahSetting);
                await db.SaveChangesAsync();
        }

        public string GetDeviceInitString(string id, int hookahVersion)
        {
            var sessionId = this.redisService.GetSessionId(id);

            var pufs = this.redisService.GetPufs(sessionId);

            var intake = pufs.Count(a => a.Type == PufType.In);
            var setting = new DeviceSetting();

            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == id);

            if (hookah?.Setting != null)
                setting = hookah.Setting;

            var percentage = 300;
            var dbSession = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);

            if (dbSession != null && dbSession.MetaData != null && dbSession.MetaData.Tobacco != null &&
                dbSession.MetaData.Tobacco.Statistics != null)
            {
                percentage = (int)dbSession.MetaData.Tobacco.Statistics.PufCount;
            }

            if (hookahVersion < 1000011)
                return setting.GetInitStringWithColor(intake);

            if (hookahVersion < 1000017)
                return setting.GetInitStringWithPercentage(intake, percentage);

            if (hookahVersion < 1000024)
                return setting.GetInitStringWithBrightness(intake, percentage);

            if (hookahVersion < 1000025)
                return setting.GetInitStringWithSessionId(intake, percentage, sessionId);

            return setting.GetInitStringWithSpeed(intake, percentage, sessionId);
        }

        public async Task<bool> UpdateDevice(int deviceId, int updateId,Models.Db.Person user,bool isAdmin)
        {
            try
            {

                if (isAdmin)
                {
                    var canUpdate = user.Hookahs.Any(a => a.Id == deviceId);
                    if (!canUpdate)
                        return false;
                }

                var hookah = await db.Hookahs.FindAsync(deviceId);
                var update = await db.Updates.FindAsync(updateId);
                var updateToken = Support.Support.RandomString(5);

                var updatePath = update.Path;

                var updateRedis = new UpdateController.UpdateRedis()
                {
                    FilePath = updatePath,
                    HookahCode = hookah.Code
                };

               this.redisService.StoreUpdate(updateToken,updateRedis);

                var msg = $"update:{updateToken}";

                await IotDeviceHelper.SendMsgToDevice(hookah.Code, msg);
            }
            catch (Exception e)
            {

               throw  new ManaException(ErrorCodes.UpdateError, "Update was not successful", e);
            }


            return true;
        }

        public string GetUpdatePath(string deviceId, string token)
        {
            var updateRedis = this.redisService.GetUpdate(token);

            if (updateRedis.HookahCode != deviceId)
            {
                throw new ManaException(ErrorCodes.UpdateError, $"Device id:{deviceId} not match with token");
            }

            return updateRedis.FilePath;
        }

        public Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds)
        {
            return this.iotService.GetOnlineStates(deviceIds);
        }

        public List<Animation> GetAnimations()
        {
            return AnimationHelper.Animations;
        }

        public Animation GetAnimation(int id)
        {
            var animations = AnimationHelper.Animations;
            if (animations.Any(a => a.Id == id))
                return animations.FirstOrDefault(a => a.Id == id);

            throw new KeyNotFoundException("Animation not found.");
        }

        public int GetDeviceVersion(string id)
        {
            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);
            return hookah?.Version ?? -1;
        }

        public DeviceSetting GetStandSettings(string id)
        {
            var hookah = db.Hookahs.Include(a => a.Setting).FirstOrDefault(a => a.Code == id);
            return hookah?.Setting;
        }

        private void SetAnimation(DeviceSetting setting, int state, int value)
        {
            switch (state)
            {
                case 0:
                    setting.IdleAnimation = value;
                    return;

                case 1:
                    setting.PufAnimation = value;
                    return;

                case 2:
                    setting.BlowAnimation = value;
                    return;
            }
        }

        private void SetBrightness(DeviceSetting setting, int state, int value)
        {
            switch (state)
            {
                case 0:
                    setting.IdleBrightness = value;
                    return;

                case 1:
                    setting.PufBrightness = value;
                    return;

                case 2:
                    setting.PufBrightness = value;
                    return;
            }
        }

        private void SetSpeed(DeviceSetting setting, int state, int value)
        {
            switch (state)
            {
                case 0:
                    setting.IdleSpeed = value;
                    return;

                case 1:
                    setting.PufSpeed = value;
                    return;

                case 2:
                    setting.PufSpeed = value;
                    return;
            }
        }

        private void SetColor(DeviceSetting setting, int state, Color color)
        {
            //TODO color change for other states
            setting.Color = color;
        }
    }
}