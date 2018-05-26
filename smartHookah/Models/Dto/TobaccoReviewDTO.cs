using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class TobaccoReviewDTO : DTO
    {
        public int Id { get; set; }

        public int Quality { get; set; }

        public int Taste { get; set; }

        public int Smoke { get; set; }

        public int Overall { get; set; }

        public string Text { get; set; }
        
        public int SmokeSessionId { get; set; }
    }
}