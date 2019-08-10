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

        public virtual TobaccoReview TobaccoReview { get; set; }

        public virtual PlaceReview PlaceReview { get; set; }

        public virtual ICollection<PipeAccessoryReview> GearReviews { get; set; }

        public virtual ICollection<Media> Medias { get; set; }

        public bool Deleted { get; set; }


        public int SmokeSessionId { get; set; }
        [ForeignKey("SmokeSessionId")]
        public virtual SmokeSession SmokeSession { get; set; }
    }
}