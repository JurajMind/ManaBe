using System.Collections.Generic;

namespace smartHookah.Services.Device
{
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
              if (animation.VersionFrom <= hookah.Version && animation.VersionTo >= hookah.Version) return;

            var msg = $"led:{(int)state}{animation.Id}";
            var sendTask = this.iotService.SendMsgToDevice(deviceId, msg);

            this.SetAnimation(hookah.Setting, (int)state, animation.Id);
            this.db.HookahSettings.AddOrUpdate(hookah.Setting);
            await Task.WhenAll(this.db.SaveChangesAsync(), sendTask);
        }

        public List<Animation> GetAnimations()
        {
            return AnimationHelper.Animations;
        }

        public int GetDeviceVersion(string id)
        {
            var hookah = db.Hookahs.FirstOrDefault(a => a.Code == id);
            return hookah?.Version ?? -1;
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
    }
}