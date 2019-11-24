using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto.Places.Reservations
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