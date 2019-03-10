using System;

namespace smartHookah.Models.Db
{
    public class Notificatiom
    {
        public int Id { get; set; }

        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        public DateTime DateTime { get; set; }
        
        public bool Read { get; set; }
        public string Msg { get; set; }

        public NotificationType Type { get; set; }

        public string JumpUrl { get; set; }
    }

    public enum NotificationType
    {
        Message = 1,
        Gamification = 2,
    }
}