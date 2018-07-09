using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

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

        public bool TobaccoMixBrand { get; set; }

        public virtual ICollection<PipeAccesory> PipeAccesories { get; set; }

        public string DisplayName { get; set; }

    }
}