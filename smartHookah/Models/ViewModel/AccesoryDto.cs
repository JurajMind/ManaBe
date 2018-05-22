namespace smartHookah.Controllers
{
    using System.Resources;

    using Order;
    using smartHookah.Models;

    public partial class PlacesController
    {
        public class AccesoryDto
        {
            public AccesoryDto()
            {
            }

            public AccesoryDto(PipeAccesory a)
            {
                this.Name = a.AccName;
                this.Brand = a.Brand.DisplayName;
                this.Id = a.Id;
                this.Picture = a.Picture;
            }

            public AccesoryDto(OwnPipeAccesories a)
                : this(a.PipeAccesory)
            {
                if (a.Price != null)
                {
                    this.Price = a.Price;
                    this.Currency = a.Currency;
                }
            }

            public string Brand { get; set; }

            public string Currency { get; set; }

            public int Id { get; set; }

            public string Name { get; set; }

            public string Picture { get; set; }

            public decimal? Price { get; set; }

            public static AccesoryDto GetDefault(string picturePath = null)
            {
                var rm = new ResourceManager("smartHookah.Resources.Order.Order", typeof(Order).Assembly);
                return new AccesoryDto
                           {
                               Name = rm.GetString("letusChoose"),
                               Brand = string.Empty,
                               Id = 0,
                               Picture = picturePath
                           };
            }
        }
    }
}