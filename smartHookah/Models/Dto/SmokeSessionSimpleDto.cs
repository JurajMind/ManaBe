using System;
using System.Linq;

namespace smartHookah.Models.Dto
{
    public class SmokeSessionSimpleDto
    {
        public string SessionId { get; set; }

        public HookahSimpleDto Hookah { get; set; }

        public SmokeSessionMetaDataDto MetaData { get; set; }

        public PlaceSimpleDTO Place { get; set; }

        public static SmokeSessionSimpleDto FromModel(SmokeSession model)
        {
            return new SmokeSessionSimpleDto()
            {
                SessionId = model.SessionId, 
                Hookah = HookahSimpleDto.FromModel(model.Hookah), 
                MetaData = SmokeSessionMetaDataDto.FromModel(model.MetaData), 
                Place = PlaceSimpleDTO.FromModel(model.Place), 

            }; 
        }
    }
}