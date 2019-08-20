using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Gear;
using smartHookah.Models.Dto.Places;

namespace smartHookah.Models.Db.Session.Dto
{
    [DataContract]
    public class SessionReviewDto
    {
        [DataMember]
        [JsonProperty("id")]
        public int? Id { get; set; }

        [DataMember]
        [JsonProperty("authorId")]
        public int? AuthorId { get; set; }

        [DataMember]
        [JsonProperty("author")]
        public string Author { get; set; }

        [DataMember]
        [JsonProperty("publishDate")]
        public DateTime PublishDate { get; set; }

        [DataMember]
        [JsonProperty("tobaccoReview")]
        public TobaccoReviewDto TobaccoReview { get; set; }

        [DataMember]
        [JsonProperty("placeReviewId")]
        public int? PlaceReviewId { get; set; }

        [DataMember]
        [JsonProperty("placeReview")]
        public PlaceReviewDto PlaceReview { get; set; }

        [DataMember]
        [JsonProperty("medias")]
        public ICollection<MediaDto> Medias { get; set; }

        [DataMember]
        [JsonProperty("smokeSessionId")]
        public int SmokeSessionId { get; set; }

        [DataMember]
        [JsonProperty("taste")]
        public int Taste { get; set; }

        [DataMember]
        [JsonProperty("smoke")]
        public int Smoke { get; set; }


        [DataMember]
        [JsonProperty("strength")]
        public int Strength { get; set; }

        [DataMember]
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [DataMember]
        [JsonProperty("smokeSession")]
        public SmokeSessionSimpleDto SmokeSession { get; set; }

        public static SessionReviewDto FromModel(SessionReview model)
        {
            return new SessionReviewDto()
            {
                Id = model.Id,
                AuthorId = model.AuthorId, 

                Taste = model?.TobaccoReview?.Taste ?? model.nsTaste ?? -1,
                Smoke = model?.TobaccoReview?.Smoke ?? model.nsSmoke ?? -1,
                Strength = model?.TobaccoReview?.Strength ?? model.nsStrength ?? -1,
                Duration = model?.TobaccoReview?.Duration ?? model.nsDuration ?? -1,

                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate, 
                TobaccoReview = TobaccoReviewDto.FromModel(model.TobaccoReview), 
                PlaceReviewId = model.PlaceReview.Id, 
                PlaceReview = PlaceReviewDto.FromModel(model.PlaceReview),
                Medias = MediaDto.FromModelList(model.Medias).ToList(), 
                SmokeSessionId = model.SmokeSessionId, 
                SmokeSession = SmokeSessionSimpleDto.FromModel(model.SmokeSession),
            }; 
        }

        public static IEnumerable<SessionReviewDto> FromModelList(IEnumerable<SessionReview> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public SessionReview ToModel()
        {
            return new SessionReview()
            {
                AuthorId = AuthorId,
                PublishDate = PublishDate,
                SmokeSessionId = SmokeSessionId,
                PlaceReview = PlaceReview?.ToModel(),
                TobaccoReview = TobaccoReview?.ToModel(),
            }; 
        }
    }

    [DataContract]
    public class SessionPlaceReviewDto
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }

        [DataMember]
        [JsonProperty("authorId")]
        public int? AuthorId { get; set; }

        [DataMember]
        [JsonProperty("author")]
        public string Author { get; set; }

        [DataMember]
        [JsonProperty("publishDate")]
        public DateTime PublishDate { get; set; }

        [DataMember]
        [JsonProperty("tobaccoReview")]
        public TobaccoReviewDto TobaccoReview { get; set; }

        [DataMember]
        [JsonProperty("gearReviews")]
        public ICollection<PipeAccessoryReviewDto> GearReviews { get; set; }

        [DataMember]
        [JsonProperty("medias")]
        public ICollection<MediaDto> Medias { get; set; }

        [DataMember]
        [JsonProperty("smokeSessionId")]
        public int SmokeSessionId { get; set; }

        [DataMember]
        [JsonProperty("smokeSession")]
        public SmokeSessionSimpleDto SmokeSession { get; set; }

        public static SessionPlaceReviewDto FromModel(SessionReview model)
        {
            if (model == null)
            {
                return null;
                
            }
            return new SessionPlaceReviewDto()
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate,
                TobaccoReview = TobaccoReviewDto.FromModel(model.TobaccoReview),
                Medias = MediaDto.FromModelList(model.Medias).ToList(),
                SmokeSessionId = model.SmokeSessionId,
                SmokeSession = SmokeSessionSimpleDto.FromModel(model.SmokeSession),
            };
        }

        public static IEnumerable<SessionPlaceReviewDto> FromModelList(IEnumerable<SessionReview> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public SessionReview ToModel()
        {
            return new SessionReview()
            {
                AuthorId = AuthorId,
                PublishDate = PublishDate,
                SmokeSessionId = SmokeSessionId,
            };
        }
    }
}