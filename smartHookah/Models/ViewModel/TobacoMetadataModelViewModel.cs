using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    using System.Collections.Generic;

    using smartHookah.Models;

    public class TobacoMetadataModelViewModel
    {
        public bool MyTobacco { get; set; }

        public bool IsTobacoMix { get; set; } = false;
        public List<SmokeMetadataModalTobacoMix> TobacoMix { get; set; } = new List<SmokeMetadataModalTobacoMix>();

        public int TobacoMixId { get; set; }

        public IEnumerable<string> TobacoBrands { get; set; }
        public string TobacoMixName { get; set;} 

        public MixLayerMethod LayerMethod { get;
            set;
        }
    }
}