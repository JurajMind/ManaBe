using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Place;

namespace smartHookah.Models.Db.Session
{
    public class SessionReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        [ForeignKey("TobaccoReview")]
        public int? TobaccoReviewId { get; set; }

        public virtual TobaccoReview TobaccoReview { get; set; }

        [ForeignKey("PlaceReview")]
        public int? PlaceReviewId { get; set; }

        public virtual PlaceReview PlaceReview { get; set; }

        public int? nsTaste { get; set; }

        public int? nsStrength { get; set; }

        public int? nsDuration { get; set; }

        public int? nsSmoke { get; set; }


        public virtual ICollection<Media> Medias { get; set; }

        public bool Deleted { get; set; }


        public int SmokeSessionId { get; set; }
        [ForeignKey("SmokeSessionId")]
        public virtual SmokeSession SmokeSession { get; set; }
    }
}