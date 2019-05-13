using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using smartHookah.Models.Dto.Places.Reservations;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PersonActiveDataDto
    {
        [DataMember, JsonProperty("Devices")]
        public ICollection<DeviceSimpleDto> Devices { get; set; }

        [DataMember, JsonProperty("ActiveSmokeSessions")]
        public ICollection<SmokeSessionSimpleDto> ActiveSmokeSessions { get; set; }

        [DataMember, JsonProperty("ActiveReservations")]
        public ICollection<ReservationDto> ActiveReservations { get; set; }

        [DataMember, JsonProperty("ActiveHookahOrders")]
        public ICollection<HookahOrderDto> ActiveHookahOrders { get; set; }

        [DataMember, JsonProperty("GameProfile")]
        public GameProfileSimpleDto GameProfile { get; set; }
    }
}