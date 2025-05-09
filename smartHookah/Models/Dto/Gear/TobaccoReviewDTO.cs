﻿using Newtonsoft.Json;
using smartHookah.Models.Db.Gear;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

        [DataMember, JsonProperty("SessionReviewId")]
        public int SessionReviewId { get; set; }

        [DataMember, JsonProperty("Medias")]
        public List<MediaDto> Medias { get; set; }

        public static TobaccoReviewDto FromModel(TobaccoReview model)
        {
            if(model == null)
            {
                return null;
            } 
return new TobaccoReviewDto()
{
Id = model.Id,

Overall = model.Overall,
Cut = model.Cut,
Strength = model.Strength,
Duration = model.Duration,
Smoke = model.Smoke,
Taste = model.Taste,
SmokeSessionId = model.SmokeSessionId ?? 0,
SessionReviewId = model?.SessionReview?.Id ?? 0,
Text = model.Text,
Medias = MediaDto.FromModelList(model.Medias).ToList(),
};
        }

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