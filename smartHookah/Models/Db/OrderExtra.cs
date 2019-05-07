using System.ComponentModel.DataAnnotations;

namespace smartHookah.Models.Db
{
    public class OrderExtra
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? PriceId { get; set; }
        public decimal Price { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; }

        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        public int? PriceGroupId { get; set; }
        public virtual PriceGroup PriceGroup { get; set; }

    }
}