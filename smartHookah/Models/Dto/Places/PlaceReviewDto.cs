using Newtonsoft.Json;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        [JsonProperty("service")]
        public int Service { get; set; }


        [DataMember]
        [JsonProperty("ambience")]
        public int Ambience { get; set; }


        [DataMember]
        [JsonProperty("overall")]
        public int Overall { get; set; }

        [DataMember]
        [JsonProperty("placeId")]
        public int? PlaceId { get; set; }


        [DataMember]
        [JsonProperty("sessionReview")]
        public SessionPlaceReviewDto SessionReview { get; set; }

        [DataMember]
        [JsonProperty("medias")]
        public ICollection<MediaDto> Medias { get; set; }

        public static PlaceReviewDto FromModel(PlaceReview model)
        {
            if (model == null)
                return null;

            return new PlaceReviewDto()
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate,
                Text = model.Text,
                PlaceId = model.PlaceId,
                SessionReview = SessionPlaceReviewDto.FromModel(model.SessionReview),
                Medias = MediaDto.FromModelList(model.Medias)?.ToList(),
                Ambience = model.Ambience,
                Overall = model.Overall,
                Service = model.Service
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
                SessionReview = SessionReview?.ToModel(),
                Ambience = Ambience,
                Overall = Overall,
                Service = Service
            };
        }
    }
}