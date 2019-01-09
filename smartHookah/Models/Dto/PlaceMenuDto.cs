using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PlaceMenuDto
    {
        [DataMember, JsonProperty("Accessories")]
        public ICollection<PipeAccesorySimpleDto> Accessories { get; set; }

        [DataMember, JsonProperty("TobaccoMixes")]
        public ICollection<TobaccoMixSimpleDto> TobaccoMixes { get; set; }

        [DataMember, JsonProperty("OrderExtras")]
        public ICollection<OrderExtraDto> OrderExtras { get; set; }
    }
}