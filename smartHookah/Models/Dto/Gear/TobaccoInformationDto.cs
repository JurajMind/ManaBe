using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Dto.Gear;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class TobaccoInformationDto
    {
        [DataMember, JsonProperty("Tobacco")]
        public TobaccoSimpleDto Tobacco { get; set; }

        [DataMember, JsonProperty("TobaccoTastes")]
        public List<TobaccoTasteDto> TobaccoTastes { get; set; }
        
        [DataMember, JsonProperty("PersonTobaccoStats")]
        public PipeAccessoryStatisticsDto PersonTobaccoStats { get; set; }

        [DataMember, JsonProperty("AllTobaccoStats")]
        public PipeAccessoryStatisticsDto AllTobaccoStats { get; set; }

        [DataMember, JsonProperty("SmokeSessions")]
        public List<SmokeSessionSimpleDto> SmokeSessions { get; set; }

        [DataMember, JsonProperty("Reviews")]
        public List<TobaccoReviewDto> Reviews { get; set; }

        public static TobaccoInformationDto FromModel(Tobacco tobacco, List<TobaccoTaste> tobaccoTastes,
            PipeAccesoryStatistics personStats, PipeAccesoryStatistics allStats, List<SmokeSession> smokeSessions, List<TobaccoReview> tobaccoReviews)
        {
            if (tobacco == null) return null;
            var tastes = new List<TobaccoTasteDto>();
            var sessions = new List<SmokeSessionSimpleDto>();
            var reviews = new List<TobaccoReviewDto>();

            if(tobaccoTastes != null)
            {
                foreach (var taste in tobaccoTastes)
                    tastes.Add(TobaccoTasteDto.FromModel(taste));
            }

            if(smokeSessions != null)
                foreach (var session in smokeSessions)
                    sessions.Add(SmokeSessionSimpleDto.FromModel(session));

            if(tobaccoReviews != null)
                foreach(var review in tobaccoReviews)
                    reviews.Add(TobaccoReviewDto.FromModel(review));

            return new TobaccoInformationDto()
            {
                Tobacco = TobaccoSimpleDto.FromModel(tobacco),
                TobaccoTastes = tastes.Count > 0 ? tastes : null,
                AllTobaccoStats = PipeAccessoryStatisticsDto.FromModel(allStats),
                PersonTobaccoStats = PipeAccessoryStatisticsDto.FromModel(personStats),
                SmokeSessions = sessions.Count > 0 ? sessions : null,
                Reviews = reviews.Count > 0 ? reviews : null
            };
        }
    }

    [DataContract]
    public class TobaccoMixInformationDto
    {
        [DataMember, JsonProperty("TobaccoMix")]
        public TobaccoMixSimpleDto TobaccoMix { get; set; }

        [DataMember, JsonProperty("TobaccosTastes")]
        public Dictionary<int, TobaccoTasteDto> TobaccosTastes { get; set; }
        
        [DataMember, JsonProperty("PersonTobaccoMixStats")]
        public PipeAccessoryStatisticsDto PersonTobaccoMixStats { get; set; }

        [DataMember, JsonProperty("AllTobaccoMixStats")]
        public PipeAccessoryStatisticsDto AllTobaccoMixStats { get; set; }

        public static TobaccoMixInformationDto FromModel(TobaccoMix tobaccoMix, Dictionary<int, TobaccoTaste> tobaccoTastes,
            PipeAccesoryStatistics personStats, PipeAccesoryStatistics allStats)
        {
            if (tobaccoMix == null || personStats == null || allStats == null) return null;
            
            var tastes = new Dictionary<int, TobaccoTasteDto>();

            if (tobaccoTastes != null)
            {
                foreach (var item in tobaccoTastes)
                    tastes.Add(item.Key, TobaccoTasteDto.FromModel(item.Value));
            }

            return new TobaccoMixInformationDto()
            {
                TobaccoMix = TobaccoMixSimpleDto.FromModel(tobaccoMix, null),
                TobaccosTastes = tastes,
                AllTobaccoMixStats = PipeAccessoryStatisticsDto.FromModel(allStats),
                PersonTobaccoMixStats = PipeAccessoryStatisticsDto.FromModel(personStats)
            };
        }
    }
}