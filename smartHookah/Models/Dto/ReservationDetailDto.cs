using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class ReservationDetailDto
    {
        [DataMember, JsonProperty("Reservation")]
        public ReservationDto Reservation { get; set; }

        [DataMember, JsonProperty("Place")]
        public PlaceDto Place { get; set; }

        [DataMember, JsonProperty("SmokeSessions")]
        public ICollection<SmokeSessionSimpleDto> SmokeSessions { get; set; }

        [DataMember, JsonProperty("Orders")]
        public ICollection<HookahOrderDto> Orders { get; set; }
    }
}