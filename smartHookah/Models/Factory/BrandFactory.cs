using System.Linq;

namespace smartHookah.Models.Factory
{
    using smartHookah.Models.Db;

    public static class BrandFactory
    {
        public static Brand FromFactory(string name, string type)
        {
            var displayArray = name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray();
            var id = new string(displayArray);
            var brand = new Brand
            {
                Name = name,
                DisplayName = name
            };

            SetBrandFlag(type, brand);

            return brand;
        }

        public static Brand SetFlag(Brand brand, string type)
        {
            SetBrandFlag(type, brand);
            return brand;
        }

        private static void SetBrandFlag(string type, Brand brand)
        {
            switch (type)
            {
                case "pipe":
                    brand.Hookah = true;
                    break;
                case "bowl":
                    brand.Bowl = true;
                    break;
                case "tobacco":
                    brand.Tobacco = true;
                    break;
                case "heatmanagement":
                    brand.HeatManagment = true;
                    break;
                case "coal":
                    brand.Coal = true;
                    break;
            }
        }
    }
}