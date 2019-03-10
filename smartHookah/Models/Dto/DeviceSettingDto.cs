using System;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    public class DeviceSettingDto
    {
        public ActionSettings PuffSettings { get; set; }
        public ActionSettings BlowSettings { get; set; }
        public ActionSettings IdleSettings { get; set; }

        public static DeviceSettingDto FromModel(DeviceSetting settings) => settings == null
            ? null
            : new DeviceSettingDto()
            {
                PuffSettings =
                    new ActionSettings()
                    {
                        Color = settings.Color,
                        AnimationId = settings.PufAnimation,
                        Brightness = settings.PufBrightness,
                        Speed = settings.PufSpeed
                    },
                BlowSettings =
                    new ActionSettings()
                    {
                        Color = settings.Color,
                        AnimationId = settings.BlowAnimation,
                        Brightness = settings.PufBrightness,
                        Speed = settings.PufSpeed
                    },
                IdleSettings =
                    new ActionSettings()
                    {
                        Color = settings.Color,
                        AnimationId = settings.IdleAnimation,
                        Brightness = settings.IdleBrightness,
                        Speed = settings.IdleSpeed
                    }
            };
    }

    public class ActionSettings
    {
        public Color Color { get; set; }
        public int AnimationId { get; set; }
        public int Brightness { get; set; }
        public int Speed { get; set; }
    }
}
