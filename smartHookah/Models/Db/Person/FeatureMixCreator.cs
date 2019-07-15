using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using smartHookah.Models.Db.Place;

namespace smartHookah.Models.Db
{
    public class FeatureMixCreator
    {
       public string Name { get; set; }
       public string Description { get; set; }

       public string Location { get; set; }

       public virtual ICollection<SocialMedia> SocialMedias { get; set; }

       public virtual ICollection<Media> Medias { get; set; }

       public string LogoPicture { get; set; }

        public virtual int PersonId { get; set; }
       public virtual Person Person { get; set; }

       [ForeignKey("Person")]
       public int Id { get; set; }

       public ICollection<Person> Followers { get; set; }
    }
}