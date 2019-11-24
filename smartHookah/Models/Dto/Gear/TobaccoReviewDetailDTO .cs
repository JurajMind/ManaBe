using Newtonsoft.Json;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Session.Dto;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto.Gear
{
    [DataContract]
    public class TobaccoReviewDetailDto : TobaccoReviewDto
    {

        [DataMember, JsonProperty("SmokeSessionId")]
        public SessionReviewDto SessionReview { get; set; }

        public new static TobaccoReviewDetailDto FromModel(TobaccoReview model) => model == null
            ? null
            : new TobaccoReviewDetailDto()
            {
                Id = model.Id,
                Overall = model.Overall,
                Cut = model.Cut,
                Strength = model.Strength,
                Duration = model.Duration,
                Smoke = model.Smoke,
                Taste = model.Taste,
                SmokeSessionId = model.SmokeSessionId ?? 0,
                SessionReview = SessionReviewDto.FromModel(model.SessionReview),
                Text = model.Text
            };
    }
}