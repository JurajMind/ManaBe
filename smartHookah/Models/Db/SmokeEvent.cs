using System;
using System.ComponentModel.DataAnnotations;

namespace smartHookah.Models
{
    public class SmokeEvent
    {
        [Key]
        public int Id { get; set; }
        public int SmokeSessionId { get; set; }
        public virtual SmokeSession SmokeSession { get; set; }
        public DateTime TimeStamp { get; set; }

        public int NumParam { get; set; }

        [MaxLength(255)]
        public string Comment { get; set; }

        public SmokeEventType Type { get; set; }
    }
}