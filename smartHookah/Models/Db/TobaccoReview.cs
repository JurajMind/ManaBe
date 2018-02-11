using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace smartHookah.Models
{
    public class TobaccoReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        public int Quality { get; set; }

        public int Taste { get; set; }

        public int Smoke { get; set; }

        public int Overall { get; set; }

        public string Text { get; set; }

        public int ReviewedTobaccoId { get; set; }
        public virtual Tobacco ReviewedTobacco { get; set; }

        
       
        public int? SmokeSessionId { get; set; }
        [ForeignKey("SmokeSessionId")]
        public virtual SmokeSession SmokeSession { get; set; }
    }
}