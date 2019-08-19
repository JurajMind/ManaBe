using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db.Gear;

namespace smartHookah.Models.Dto.Gear
{
    [DataContract]
    public class TobaccoReviewDto
    {
        [DataMember, JsonProperty("Id")]
        public int Id { get; set; }

        [DataMember, JsonProperty("Cut")]
        public int Cut { get; set; }

        [DataMember, JsonProperty("Taste")]
        public int Taste { get; set; }

        [DataMember, JsonProperty("Smoke")]
        public int Smoke { get; set; }

        [DataMember, JsonProperty("Strength")]
        public int Strength { get; set; }

        [DataMember, JsonProperty("Duration")]
        public int Duration { get; set; }

        [DataMember, JsonProperty("Overall")]
        public double Overall { get; set; }

        [DataMember, JsonProperty("Text")]
        public string Text { get; set; }

        [DataMember, JsonProperty("SmokeSessionId")]
        public int SmokeSessionId { get; set; }

        public static TobaccoReviewDto FromModel(TobaccoReview model) => model == null
            ? null
            : new TobaccoReviewDto()
            {
                Id = model.Id,

                Overall = model.Overall,
                Cut = model.Cut,
                Strength = model.Strength,
                Duration = model.Duration,
                Smoke = model.Smoke,
                Taste = model.Taste,

                Text = model.Text
            };

        public static IEnumerable<TobaccoReviewDto> FromModelList(IEnumerable<TobaccoReview> model)
        {
            if (model == null) yield break;

            foreach (var item in model)
                yield return FromModel(item);
        }

        internal TobaccoReview ToModel()
        {
            return new TobaccoReview()
            {
                Id = Id,
                Text = Text,
                Taste = Taste,
                Smoke = Smoke,
                Strength = Strength,
            };
        }
    }
}