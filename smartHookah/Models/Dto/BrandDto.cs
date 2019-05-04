using System.Collections.Generic;
using System.Linq;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    public class BrandDto
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Picture { get; set; }

        public bool Tobacco { get; set; }

        public bool Bowl { get; set; }

        public bool Hookah { get; set; }

        public bool Coal { get; set; }

        public bool HeatManagment { get; set; }

        public bool TobaccoMixBrand { get; set; }

        public IList<PipeAccesorySimpleDto> PipeAccessories { get; set; }

        public IList<SocialMedia> SocialMedias { get; set; }

        public IList<MediaDto> Medias { get; set; }

        public string DisplayName { get; set; }

        public static BrandDto FromModel(Brand model)
        {
            return new BrandDto()
            {
                Name = model.Name, 
                Url = model.Url, 
                Picture = model.Picture, 
                Tobacco = model.Tobacco, 
                Bowl = model.Bowl, 
                Hookah = model.Hookah, 
                Coal = model.Coal, 
                HeatManagment = model.HeatManagment, 
                TobaccoMixBrand = model.TobaccoMixBrand, 
                PipeAccessories = PipeAccesorySimpleDto.FromModelList(model.PipeAccessories).ToList(), 
                SocialMedias = model.SocialMedias.ToList(), 
                Medias = MediaDto.FromModelList(model.Medias).ToList(), 
                DisplayName = model.DisplayName, 
            }; 
        }
    }
}