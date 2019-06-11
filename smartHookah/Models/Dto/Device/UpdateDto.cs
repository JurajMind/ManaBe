using System;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto.Device
{
    public class UpdateDto
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public DateTime ReleseDate { get; set; }

        public string ReleseNote { get; set; }

        public UpdateType Type { get; set; }

        public static UpdateDto FromModel(Update model)
        {
            return new UpdateDto()
            {
                Id = model.Id, 
                Version = model.Version, 
                ReleseDate = model.ReleseDate.Date, 
                ReleseNote = model.ReleseNote, 
                Type = model.Type, 
            }; 
        }

        public Update ToModel()
        {
            return new Update()
            {
                Id = Id, 
                Version = Version, 
                ReleseDate = ReleseDate, 
                ReleseNote = ReleseNote, 
                Type = Type, 
            }; 
        }
    }
}