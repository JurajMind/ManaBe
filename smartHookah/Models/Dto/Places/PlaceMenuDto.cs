using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    using smartHookah.Controllers;

    [DataContract]
    public class PlaceMenuDto
    {
        [DataMember, JsonProperty("Accessories")]
        public ICollection<PipeAccesorySimpleDto> Accessories { get; set; }

        [DataMember, JsonProperty("TobaccoMixes")]
        public ICollection<TobaccoMixSimpleDto> TobaccoMixes { get; set; }

        [DataMember, JsonProperty("OrderExtras")]
        public ICollection<OrderExtraDto> OrderExtras { get; set; }

        [DataMember, JsonProperty("BasePrice")]
        public decimal BasePrice { get; set; }

        [DataMember, JsonProperty("Currency")]
        public string Currency { get; set; }

        [DataMember, JsonProperty("PriceGroup")]
        public List<PriceGroupDto> PriceGroup { get; set; }

        [DataMember, JsonProperty("Prices")]
        public List<PriceGroupItems> PriceMatrix { get; set; }
    }

    public class PriceGroupItems
    {
        [DataMember, JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [DataMember, JsonProperty("Price")]
        public Dictionary<int, decimal> Prices { get; set; }
    }

}