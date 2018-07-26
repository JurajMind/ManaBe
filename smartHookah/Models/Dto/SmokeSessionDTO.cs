using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Models.Redis;

namespace smartHookah.Models.Dto
{
    public class SmokeSessionDTO
    {
    }

    public class ValidationDTO : DTO
    {
        public String Id { get; set; }
        public SessionState Flag { get; set; }
    }

    public enum SessionState{ Live, Completed }

    public class InitDataDTO : DTO
    {
        public DynamicSmokeStatistic RedisStatistics { get; set; }
        public SmokeSessionStatistics SessionStatistics { get; set; }
        public SmokeSessionMetaData SessionMetaData { get; set; }
        public StandSettings StandSettings { get; set; }
    }

    public class StandSettings
    {
        public ActionSettings PuffSettings { get; set; }
        public ActionSettings BlowSettings { get; set; }
        public ActionSettings IdleSettings { get; set; }

        public StandSettings(HookahSetting settings)
        {
            PuffSettings = new ActionSettings()
            {
                Color = settings.Color,
                AnimationId = settings.PufAnimation,
                Brightness = settings.PufBrightness,
                Speed = settings.PufSpeed
            };
            BlowSettings = new ActionSettings()
            {
                Color = settings.Color,
                AnimationId = settings.BlowAnimation,
                Brightness = settings.PufBrightness,
                Speed = settings.PufSpeed
            };
            IdleSettings = new ActionSettings()
            {
                Color = settings.Color,
                AnimationId = settings.IdleAnimation,
                Brightness = settings.IdleBrightness,
                Speed = settings.IdleSpeed
            };
        }
    }

    public class ActionSettings
    {
        public Color Color { get; set; }
        public int AnimationId { get; set; }
        public int Brightness { get; set; }
        public int Speed { get; set; }
    }

}