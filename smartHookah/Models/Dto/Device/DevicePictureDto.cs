using smartHookah.Models.Db.Device;

namespace smartHookah.Models.Dto.Device
{
    public class DevicePictureDto
    {
        public int Id { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string InlinePicture { get; private set; }

        public static DevicePictureDto FromModel(StandPicture model)
        {
            return new DevicePictureDto()
            {
                Id = model.Id,
                Width = model.Width,
                Height = model.Height,
                InlinePicture = model.HtmlString,
            };
        }

        public StandPicture ToModel()
        {
            return new StandPicture()
            {
                Id = Id,
                Width = Width,
                Height = Height,
            };
        }
    }
}