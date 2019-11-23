using smartHookah.Models.Db.Device;

namespace smartHookah.Models.Db
{
    public class DeviceSetting
    {
        public DeviceSetting()
        {
        }

        public DeviceSetting(DeviceSetting setting)
        {
            this.Color = setting.Color;
            this.PictureId = setting.PictureId;

            //Animations
            this.IdleAnimation = setting.IdleAnimation;
            this.PufAnimation = setting.PufAnimation;
            this.BlowAnimation = setting.BlowAnimation;

            //Br
            this.IdleBrightness = setting.IdleBrightness;
            this.PufBrightness = setting.PufBrightness;

            //Speed
            this.IdleSpeed = setting.IdleSpeed;
            this.PufSpeed = setting.PufSpeed;
        }

        public void SetAnimation(int index, int value)
        {
            switch (index)
            {
                case 0:
                    IdleAnimation = value;
                    return;

                case 1:
                    PufAnimation = value;
                    return;

                case 2:
                    BlowAnimation = value;
                    return;
            }
        }

        public void SetBrightness(int index, int value)
        {
            if (index == 0)
            {
                IdleBrightness = value;
            }
            else
            {
                PufBrightness = value;
            }
        }

        public int Id { get; set; }

        public int PufAnimation { get; set; } = 1;

        public int BlowAnimation { get; set; } = 2;

        public int IdleAnimation { get; set; } = 3;

        public int IdleBrightness { get; set; } = 20;

        public int PufBrightness { get; set; } = 255;

        public int IdleSpeed { get; set; } = 70;

        public int PufSpeed { get; set; } = 100;

        public Color Color { get; set; } = new Color();

        public Color PufColor { get; set; } = new Color();

        public Color BlowColor { get; set; } = new Color();

        public virtual DevicePreset DevicePreset { get; set; }

        public string GetInitString()
        {
            return $"{PufAnimation},{BlowAnimation},{IdleAnimation},";
        }

        public string GetInitStringWithColor(int intake)
        {
            return
                $"{PufAnimation},{BlowAnimation},{IdleAnimation},{intake},{Color.Hue:000},{Color.Saturation:000},{Color.Value:000},";
        }

        public string GetInitStringWithPercentage(int intake, int percentage)
        {
            return
                $"{PufAnimation},{BlowAnimation},{IdleAnimation},{intake},{percentage},{Color.Hue:000},{Color.Saturation:000},{Color.Value:000},";
        }

        public string GetInitStringWithBrightness(int intake, int percentage)
        {
            return
                $"{PufAnimation},{BlowAnimation},{IdleAnimation},{intake},{percentage},{Color.Hue:000},{Color.Saturation:000},{Color.Value:000},{IdleBrightness},{PufBrightness},{0},";
        }

        public int PictureId { get; set; }
        public virtual StandPicture Picture { get; set; }

        public string GetInitStringWithSessionId(int intake, int percentage, string sessionId)
        {
            return $"{GetInitStringWithBrightness(intake, percentage)},{sessionId},";
        }

        public string GetInitStringWithSpeed(int intake, int percentage, string sessionId)
        {
            return $"{GetInitStringWithBrightness(intake, percentage)}{IdleSpeed},{PufSpeed},{sessionId},";
        }

        public void SetSpeed(int speedIndex, int speedValue)
        {
            if (speedIndex == 0)
            {
                IdleSpeed = speedValue;
            }
            else
            {
                PufSpeed = speedValue;
            }
        }

        public void Change(DeviceSetting defaultAnimation)
        {
            this.BlowAnimation = defaultAnimation.BlowAnimation;
            this.Color = defaultAnimation.Color;
            this.IdleAnimation = defaultAnimation.IdleAnimation;
            this.IdleBrightness = defaultAnimation.IdleBrightness;
            this.IdleSpeed = defaultAnimation.IdleSpeed;
            this.PufAnimation = defaultAnimation.PufAnimation;
            this.PufSpeed = defaultAnimation.PufSpeed;
            this.PufBrightness = defaultAnimation.PufBrightness;
        }

        public string GetInitMultipleColor(int intake, int percentage, string sessionId)
        {
            return $"{PufAnimation},{BlowAnimation},{IdleAnimation},{intake},{percentage}," +
                   $"{Color.Hue:000},{Color.Saturation:000},{Color.Value:000}," +
                   $"{PufColor.Hue:000},{PufColor.Saturation:000},{PufColor.Value:000}," +
                   $"{BlowColor.Hue:000},{BlowColor.Saturation:000},{BlowColor.Value:000}," +
                   $"{IdleBrightness},{PufBrightness},{0}," +
                   $"{IdleSpeed},{PufSpeed},{sessionId},";
        }
    }

    public class Color
    {
        public byte Hue { get; set; }

        public byte Saturation { get; set; }

        public byte Value { get; set; }
    }
}