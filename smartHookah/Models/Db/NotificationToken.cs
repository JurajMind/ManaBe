using System.ComponentModel.DataAnnotations;

namespace smartHookah.Models.Db
{
    public class NotificationToken
    {
        [Key]
        public int Id { get; set; }
        public int PersonId { get; set; }
        [Required]
        public virtual Person  Person { get; set; }
        public string Token { get; set; }
    }
}