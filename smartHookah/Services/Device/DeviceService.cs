using System;
using System.Collections.Generic;

namespace smartHookah.Services.Device
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.TeamFoundation.VersionControl.Client;

    using smartHookah.Helpers;
    using smartHookah.Models;

    public class DeviceService : IDeviceService
    {
        private readonly SmartHookahContext db;

        private readonly IIotService iotService;

        public DeviceService(SmartHookahContext db, IIotService iotService)
        {
            this.db = db;
            this.iotService = iotService;
        }

        public async Task SetAnimation(string deviceId, Animation animation, PufType state)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            if (animation.VersionFrom != 0 && animation.VersionTo != 0)
              if (animation.VersionFrom >= hookah.Version || animation.VersionTo <= hookah.Version)
                    throw new NotSupportedException($"Animation {animation.DisplayName} not supported by your Hookah OS version.");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"led:{(int)state}{animation.Id}");

            this.SetAnimation(hookah.Setting, (int)state, animation.Id);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetBrightness(string deviceId, int brightness, PufType state)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"br:{(int)state}{brightness}");

            this.SetBrightness(hookah.Setting, (int)state, brightness);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetSpeed(string deviceId, int speed, PufType state)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"spd:{(int)state}{speed}");

            this.SetSpeed(hookah.Setting, (int)state, speed);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task SetColor(string deviceId, Color color, PufType state)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            var sendTask = this.iotService.SendMsgToDevice(deviceId, $"clr:{color.Hue:000},{color.Saturation:000},{color.Value:000}");

            this.SetColor(hookah.Setting, (int)state, color);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public async Task Sleep(string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "slp:");
        }

        public async Task Restart(string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "restart:");
        }

        public async Task SetMode(string deviceId, int mode)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, $"mode:{mode}");
        }

        public async Task ShowQrCode(string deviceId)
        {
            var hookah = this.db.Hookahs.FirstOrDefault(a => a.Code == deviceId);

            if (hookah == null) throw new ItemNotFoundException($"Device with id {deviceId} not found");

            await this.iotService.SendMsgToDevice(deviceId, "qrcode:");
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

            throw new ItemNotFoundException("Animation not found.");
        }

        public int GetDeviceVersion(string id)
        {
            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);
            return hookah?.Version ?? -1;
        }

        public HookahSetting GetStandSettings(string id)
        {
            var hookah = db.Hookahs.Include(a => a.Setting).FirstOrDefault(a => a.Code == id);
            return hookah?.Setting;
        }

        private void SetAnimation(HookahSetting setting, int state, int value)
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

        private void SetBrightness(HookahSetting setting, int state, int value)
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

        private void SetSpeed(HookahSetting setting, int state, int value)
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

        private void SetColor(HookahSetting setting, int state, Color color)
        {
            //TODO color change for other states
            setting.Color = color;
        }
    }
}