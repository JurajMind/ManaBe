using System.ComponentModel.DataAnnotations;

namespace smartHookah.Models.Db
{
    public class NotificationToken
    {
        public int PersonId { get; set; }
       
        public virtual Person  Person { get; set; }
        [Key]
        [MaxLength(255)]
        public string Token { get; set; }
    }
}