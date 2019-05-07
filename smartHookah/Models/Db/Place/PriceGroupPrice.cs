using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class PriceGroupPrice
    {
        [Key, Column( Order = 0)]
        public int OwnPipeAccesoriesId { get; set; }

        public virtual OwnPipeAccesories PipeAccesorie { get; set; }

      
        public virtual PriceGroup PriceGroup { get; set; }
        [Key, Column(Order = 1)]
        public int PriceGroupId { get; set; }

        public decimal Price { get; set; }


    }
}