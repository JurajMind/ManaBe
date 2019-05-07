using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PipeAccessoryDetailsDto
    {
        [DataMember, JsonProperty("UsedByPerson")]
        public int UsedByPerson { get; set; }

        [DataMember, JsonProperty("UsedWith")]
        public ICollection<UsedWithDto> UsedWith { get; set; }

        [DataMember, JsonProperty("OwnedByPersons")]
        public int OwnedByPersons { get; set; }

        [DataMember, JsonProperty("OwnedByPlaces")]
        public int OwnedByPlaces { get; set; }
    }

    [DataContract]
    public class UsedWithDto
    {
        [DataMember, JsonProperty("Accessory")]
        public PipeAccesorySimpleDto Accessory { get; set; }

        [DataMember, JsonProperty("UsedCount")]
        public int UsedCount { get; set; }
    }
}