using smartHookah.Models.Db.Place;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace smartHookah.Models.Db
{
    public class Brand
    {
        [Key]
        public string Name { get; set; }

        public string Url { get; set; }

        public string Picture { get; set; }

        public bool Tobacco { get; set; }

        public bool Bowl { get; set; }

        public bool Hookah { get; set; }

        public bool Coal { get; set; }

        public bool HeatManagment { get; set; }

        public bool TobaccoMixBrand { get; set; }

        public virtual ICollection<PipeAccesory> PipeAccessories { get; set; }

        public virtual ICollection<SocialMedia> SocialMedias { get; set; }
        public virtual ICollection<Media> Medias { get; set; }


        public string DisplayName { get; set; }

        public ICollection<BrandTranslation> DbDescription { get; set; }

    }
}