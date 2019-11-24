using smartHookah.Models.Db.Session;
using System;
using System.Collections.Generic;

namespace smartHookah.Models.Db.Place
{
    public class PlaceReview
    {
        public int Id { get; set; }

        public int? AuthorId { get; set; }
        public virtual Person Author { get; set; }

        public DateTime PublishDate { get; set; }

        public bool Deleted { get; set; }

        public string Text { get; set; }

        public int? PlaceId { get; set; }

        public virtual Place Place { get; set; }


        public virtual SessionReview SessionReview { get; set; }

        public virtual ICollection<Media> Medias { get; set; }

        public int Service { get; set; }

        public int Ambience { get; set; }

        public int Overall { get; set; }

    }
}
