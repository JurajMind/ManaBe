﻿using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Services.Messages;
using smartHookah.Services.Redis;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace smartHookah.Services.Device
{


    public class DeviceService : IDeviceService
    {
        private readonly SmartHookahContext db;

        private readonly IIotService iotService;

        private readonly IRedisService redisService;

        private readonly ISignalNotificationService _signalNotificationService;



        public DeviceService(SmartHookahContext db, IIotService iotService, IRedisService redisService,
            ISignalNotificationService signalNotificationService)
        {
            this.db = db;
            this.iotService = iotService;
            this.redisService = redisService;
            this._signalNotificationService = signalNotificationService;
        }

        public async Task SetAnimation(string deviceId, Animation animation, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            if (animation.VersionFrom != 0 && animation.VersionTo != 0)
                if (animation.VersionFrom >= hookah.Version || animation.VersionTo <= hookah.Version)
                    throw new NotSupportedException(
                        $"Animation {animation.DisplayName} not supported by your Hookah OS version.");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"led:{(int)state},{animation.Id},");

            this.SetAnimation(hookah.Setting, (int)state, animation.Id);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this._signalNotificationService.SessionSettingsChanged(deviceId, hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetBrightness(string deviceId, int brightness, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"br:{(int)state},{brightness},");

            this.SetBrightness(hookah.Setting, (int)state, brightness);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this._signalNotificationService.SessionSettingsChanged(deviceId, hookah.Setting);
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
            this._signalNotificationService.SessionSettingsChanged(deviceId, hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetColor(string deviceId, Color color, PufType state)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");
            Task sendTask;
            if (hookah.Version < 1000035)
                sendTask = this.iotService.SendMsgToDevice(deviceId,
                    $"clr:{color.Hue:000},{color.Saturation:000},{color.Value:000}");
            else
            {
                sendTask = this.iotService.SendMsgToDevice(deviceId,
                    $"clr:{(int)state},{color.Hue:000},{color.Saturation:000},{color.Value:000}");
            }

            this.SetColor(hookah.Setting, (int)state, color);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            this._signalNotificationService.SessionSettingsChanged(deviceId, hookah.Setting);
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

        public async Task Ping(string deviceId)
        {
            var hookah = this.getDevice(deviceId);

            if (hookah == null) throw new KeyNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "ping:");
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

            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            var hookahSetting = hookah?.Setting;

            if (hookahSetting == null)
                return;

            hookahSetting.IdleAnimation = _idleAnimation;
            hookahSetting.BlowAnimation = _blowAnimation;
            hookahSetting.PufAnimation = _pufAnimation;
            hookahSetting.IdleBrightness = _idleBr;
            hookahSetting.PufBrightness = _pufBr;

            hookah.Setting = hookahSetting;
            db.Hookahs.AddOrUpdate(hookah);
            this._signalNotificationService.SessionSettingsChanged(deviceId, hookahSetting);
            await db.SaveChangesAsync();
        }

        public string GetDeviceInitString(string id, int hookahVersion)
        {
            var sessionId = this.redisService.GetSessionId(id);

            var pufs = this.redisService.GetPuffs(sessionId);

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


            if (hookahVersion < 1000035)
                return setting.GetInitStringWithSpeed(intake, percentage, sessionId);

            return setting.GetInitMultipleColor(intake, percentage, sessionId);
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

        public async Task SetPercentage(string code, int pufCount)
        {
            await this.iotService.SendMsgToDevice(code, $"stat:{pufCount}:");

        }

        public ICollection<Models.Db.SmokeSession> Sessions(int id, int pageSize, int page)
        {
            var sessions = this.db.SmokeSessions.Where(a => a.HookahId == id).OrderByDescending(a => a.Id).Skip(pageSize * page).Take(pageSize);
            return sessions.ToList();
        }

        public async  Task<bool> DeleteDevice(string code)
        {
            var dbDevice = this.db.Hookahs.FirstOrDefault(a => a.Code == code);

            if (dbDevice == null)
            {
                throw new ManaException(ErrorCodes.DeviceNotFound);
            }

            this.db.HookahSettings.Remove(dbDevice.Setting);
            this.db.Hookahs.Remove(dbDevice);
            this.db.SaveChanges();

            await this.iotService.DeleteDevice(dbDevice.Code);
            return true;
        }

        public Task<Dictionary<string, bool>> GetOnlineStates(IEnumerable<string> deviceIds)
        {
            return this.iotService.GetOnlineStates(deviceIds.ToList());
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
            switch (state)
            {
                case 0:
                    setting.Color = color;
                    break;

                case 1:
                    setting.PufColor = color;
                    break;

                case 2:
                    setting.BlowColor = color;
                    break;
            }
        }

    }
}