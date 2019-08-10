using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Gear;

namespace smartHookah.Models.Dto
{
    public class TobaccoReviewDTO : DTO
    {
        public int Id { get; set; }

        public int Quality { get; set; }

        public int Taste { get; set; }

        public int Smoke { get; set; }

        public double Overall { get; set; }

        public string Text { get; set; }
        
        public int SmokeSessionId { get; set; }
    }

    [DataContract]
    public class TobaccoReviewDto
    {
        [DataMember, JsonProperty("Id")]
        public int Id { get; set; }

        [DataMember, JsonProperty("Quality")]
        public int Quality { get; set; }

        [DataMember, JsonProperty("Taste")]
        public int Taste { get; set; }

        [DataMember, JsonProperty("Smoke")]
        public int Smoke { get; set; }

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
                Quality = model.Quality,
                Smoke = model.Smoke,
                // SmokeSessionId = model.SmokeSession.Id,
                Taste = model.Taste,
                Text = model.Text
            };

        public static IEnumerable<TobaccoReviewDto> FromModelList(IEnumerable<TobaccoReview> model)
        {
            if (model == null) yield break;

            foreach (var item in model)
                yield return FromModel(item);
        }
    }
}