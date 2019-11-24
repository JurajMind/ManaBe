using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db.Gear
{
    public class TobaccoReview : PipeAccessoryReview
    {
        public int Taste { get; set; }

        public int Smoke { get; set; }

        public int Cut { get; set; }

        public int Strength { get; set; }

        public int Duration { get; set; }

        [NotMapped]
        public int ReviewedTobaccoId
        {
            get => base.AccessorId;
            set => base.AccessorId = value;
        }

        [NotMapped]
        public Tobacco ReviewedTobacco
        {
            get => base.Accessor as Tobacco;
            set => base.Accessor = value;
        }

    }

    public class PipeAccessoryReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        public bool Deleted { get; set; }

        public string Text { get; set; }

        public int AccessorId { get; set; }

        public double Overall { get; set; }

        public virtual PipeAccesory Accessor { get; set; }

        public virtual SessionReview SessionReview { get; set; }

        public int? SmokeSessionId { get; set; }

        public virtual ICollection<Media> Medias { get; set; }
    }
}