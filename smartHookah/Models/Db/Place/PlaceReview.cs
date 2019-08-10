using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Session;

namespace smartHookah.Models.Db.Place
{
    public class PlaceReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        public bool Deleted { get; set; }

        public string Text { get; set; }

        public int? PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }

        public int? SessionReviewId { get; set; }
        public virtual SessionReview SessionReview { get; set; }

        public virtual ICollection<Media> Medias{ get; set; }
    }
}
