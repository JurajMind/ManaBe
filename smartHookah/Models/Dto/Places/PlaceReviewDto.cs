using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session.Dto;

namespace smartHookah.Models.Dto.Places
{
    [DataContract]
    public class PlaceReviewDto
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
        [JsonProperty("text")]
        public string Text { get; set; }

        [DataMember]
        [JsonProperty("placeId")]
        public int? PlaceId { get; set; }

        [DataMember]
        [JsonProperty("place")]
        public PlaceDto Place { get; set; }
        
        [DataMember]
        [JsonProperty("sessionReview")]
        public SessionReviewDto SessionReview { get; set; }

        [DataMember]
        [JsonProperty("medias")]
        public ICollection<Media> Medias { get; set; }

        public static PlaceReviewDto FromModel(PlaceReview model)
        {
            return new PlaceReviewDto()
            {
                Id = model.Id, 
                AuthorId = model.AuthorId, 
                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate, 
                Text = model.Text, 
                PlaceId = model.PlaceId, 
                Place = PlaceDto.FromModel(model.Place), 
                SessionReview = SessionReviewDto.FromModel(model.SessionReview), 
                Medias = model.Medias, 
            }; 
        }

        public static IEnumerable<PlaceReviewDto> FromModelList(IEnumerable<PlaceReview> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public PlaceReview ToModel()
        {
            return new PlaceReview()
            {
                Id = Id, 
                AuthorId = AuthorId,
                PublishDate = PublishDate, 
                Text = Text, 
                PlaceId = PlaceId, 
                SessionReview = SessionReview.ToModel(), 
                Medias = Medias, 
            }; 
        }
    }
}