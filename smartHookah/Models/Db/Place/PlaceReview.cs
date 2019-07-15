using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db.Place
{
    public class PlaceReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        public string Text { get; set; }

        public int? PlaceId { get; set; }
        [ForeignKey("PlaceId")]
        public virtual Place Place { get; set; }
    }
}
