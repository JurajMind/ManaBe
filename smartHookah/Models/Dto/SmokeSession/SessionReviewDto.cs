using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Places;

namespace smartHookah.Models.Db.Session.Dto
{
    [DataContract]
    public class SessionReviewDto
    {
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
        [JsonProperty("gearReviews")]
        public ICollection<PipeAccessoryReview> GearReviews { get; set; }

        [DataMember]
        [JsonProperty("medias")]
        public ICollection<MediaDto> Medias { get; set; }

        [DataMember]
        [JsonProperty("smokeSessionId")]
        public int SmokeSessionId { get; set; }

        [DataMember]
        [JsonProperty("smokeSession")]
        public SmokeSessionSimpleDto SmokeSession { get; set; }

        public static SessionReviewDto FromModel(SessionReview model)
        {
            return new SessionReviewDto()
            {
                AuthorId = model.AuthorId, 
                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate, 
                TobaccoReview = TobaccoReviewDto.FromModel(model.TobaccoReview), 
                PlaceReviewId = model.PlaceReview.Id, 
                PlaceReview = PlaceReviewDto.FromModel(model.PlaceReview), 
                GearReviews = model.GearReviews, 
                Medias = MediaDto.FromModelList(model.Medias).ToList(), 
                SmokeSessionId = model.SmokeSessionId, 
                SmokeSession = SmokeSessionSimpleDto.FromModel(model.SmokeSession), 
            }; 
        }

        public SessionReview ToModel()
        {
            return new SessionReview()
            {
                AuthorId = AuthorId,
                PublishDate = PublishDate,
                PlaceReview = PlaceReview.ToModel(), 
                GearReviews = GearReviews, 
                SmokeSessionId = SmokeSessionId,
            }; 
        }
    }
}