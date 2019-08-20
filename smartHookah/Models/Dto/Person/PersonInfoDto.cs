using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PersonInfoDto
    {
        [DataMember, JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [DataMember, JsonProperty("Email")]
        public string Email { get; set; }

        [DataMember, JsonProperty("ManagedPlaces")]
        public ICollection<PlaceSimpleDto> ManagedPlaces { get; set; }

        [DataMember, JsonProperty("Roles")]
        public List<string> Roles { get; set; }

        [DataMember, JsonProperty("PersonId")]
        public int PersonId { get; set; }

    }
}