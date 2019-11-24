using smartHookah.Models;
using System.Collections.Generic;

namespace smartHookah.Controllers
{
    public class GetPlaceMenuModel
    {
        public List<PlacesController.AccesoryDto> Hookah { get; set; }
        public List<PlacesController.AccesoryDto> Bowl { get; set; }
        public List<PlacesController.AccesoryDto> Tobacco { get; set; }

        public List<PlacesController.AccesoryDto> Mixes { get; set; }
        public List<OrderExtraDto> Extra { get; set; }

        public decimal BasePrice { get; set; }
        public List<PlacesController.AccesoryDto> AlternativeTobacco { get; set; }
        public List<PriceGroupDto> PriceGroup { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> PriceMatrix { get; set; }
    }

}