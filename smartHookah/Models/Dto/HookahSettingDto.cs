using System;

namespace smartHookah.Models.Dto
{
    public class HookahSettingDto
    {
        public int Id { get; set; }

        public int PufAnimation { get; set; }

        public int BlowAnimation { get; set; }

        public int IdleAnimation { get; set; }

        public int IdleBrightness { get; set; }

        public int PufBrightness { get; set; }

        public int IdleSpeed { get; set; }

        public int PufSpeed { get; set; }

        public BtState Bt { get; set; }

        public ColorDto Color { get; set; }

        public int PictureId { get; set; }


        public static HookahSettingDto FromModel(HookahSetting model)
        {
            return new HookahSettingDto()
            {
                Id = model.Id, 
                PufAnimation = model.PufAnimation, 
                BlowAnimation = model.BlowAnimation, 
                IdleAnimation = model.IdleAnimation, 
                IdleBrightness = model.IdleBrightness, 
                PufBrightness = model.PufBrightness, 
                IdleSpeed = model.IdleSpeed, 
                PufSpeed = model.PufSpeed, 
                Bt = model.Bt, 
                Color = ColorDto.FromModel(model.Color), 
                PictureId = model.PictureId, 
            }; 
        }

    }

    public class ColorDto
    {
        public byte Hue { get; set; }

        public byte Saturation { get; set; }

        public byte Value { get; set; }

        public static ColorDto FromModel(Color model)
        {
            return new ColorDto()
            {
                Hue = model.Hue, 
                Saturation = model.Saturation, 
                Value = model.Value, 
            }; 
        }

        public Color ToModel()
        {
            return new Color()
            {
                Hue = Hue, 
                Saturation = Saturation, 
                Value = Value, 
            }; 
        }
    }
}