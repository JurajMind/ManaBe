using smartHookah.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace smartHookah.Models.Db.Gear
{
    public class PipeAccessoryReviewDto
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }

        public String Author { get; set; }

        public DateTime PublishDate { get; set; }

        public bool Deleted { get; set; }

        public string Text { get; set; }

        public int AccessorId { get; set; }

        public double Overall { get; set; }


        public int? SessionReviewId { get; set; }

        public int? SmokeSessionId { get; set; }

        public ICollection<MediaDto> Medias { get; set; }

        public static PipeAccessoryReviewDto FromModel(PipeAccessoryReview model)
        {
            return new PipeAccessoryReviewDto()
            {
                Id = model.Id,
                AuthorId = model.AuthorId,
                Author = model.Author.DisplayName,
                PublishDate = model.PublishDate,
                Deleted = model.Deleted,
                Text = model.Text,
                AccessorId = model.AccessorId,
                Overall = model.Overall,
                SessionReviewId = model?.SessionReview?.Id,
                SmokeSessionId = model.SmokeSessionId,
                Medias = MediaDto.FromModelList(model.Medias).ToList(),
            };
        }

        public static IEnumerable<PipeAccessoryReviewDto> FromModelList(ICollection<PipeAccessoryReview> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
                yield return FromModel(item);
        }

        public PipeAccessoryReview ToModel()
        {
            return new PipeAccessoryReview()
            {
                Id = Id,
                AuthorId = AuthorId,
                PublishDate = PublishDate,
                Deleted = Deleted,
                Text = Text,
                AccessorId = AccessorId,
                Overall = Overall,
                SmokeSessionId = SmokeSessionId,
            };
        }
    }
}