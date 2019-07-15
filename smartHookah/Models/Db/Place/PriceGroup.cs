using System.Collections.Generic;

namespace smartHookah.Models.Db
{
    public class PriceGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<OwnPipeAccesories> Items { get; set; }

        public int PlaceId { get; set; }
        public virtual Place.Place Place { get; set; }

        public decimal Price { get; set; }
    }
}