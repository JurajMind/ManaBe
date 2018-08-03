using System;
using System.Linq;

namespace smartHookah.Models.Dto
{
    using smartHookah.Models.Redis;

    public class SmokeSessionSimpleDto
    {
        public string SessionId { get; set; }

        public HookahSimpleDto Hookah { get; set; }

        public DynamicSmokeStatisticDto Statistic { get; set; }

        public SmokeSessionMetaDataDto MetaData { get; set; }

        public PlaceSimpleDto Place { get; set; }

        public static SmokeSessionSimpleDto FromModel(SmokeSession model)
        {
            return new SmokeSessionSimpleDto()
            {
                SessionId = model.SessionId, 
                Hookah = HookahSimpleDto.FromModel(model.Hookah), 
                MetaData = SmokeSessionMetaDataDto.FromModel(model.MetaData), 
                Place = PlaceSimpleDto.FromModel(model.Place), 
                Statistic = new DynamicSmokeStatisticDto(model.DynamicSmokeStatistic)

            }; 
        }
    }
}