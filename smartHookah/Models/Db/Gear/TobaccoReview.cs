using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db.Gear
{
    public class TobaccoReview : PipeAccessoryReview
    {
        public int Quality { get; set; }

        public int Taste { get; set; }

        public int Smoke { get; set; }

        [NotMapped]
        public int ReviewedTobaccoId {
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

        public string Text { get; set; }

        public int AccessorId { get; set; }

        public double Overall { get; set; }

        public virtual PipeAccesory Accessor { get; set; }
        public int? SmokeSessionId { get; set; }
        [ForeignKey("SmokeSessionId")]
        public virtual SmokeSession SmokeSession { get; set; }
    }
}