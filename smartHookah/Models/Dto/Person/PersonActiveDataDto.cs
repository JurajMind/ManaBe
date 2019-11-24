using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using smartHookah.Models.Dto.Device;
using smartHookah.Models.Dto.Places.Reservations;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PersonActiveDataDto
    {
        [DataMember, JsonProperty("Devices")]
        public ICollection<DeviceSimpleDto> Devices { get; set; }

        [DataMember, JsonProperty("UpdateInfo")]
        public DeviceUpdateInfoDto UpdateInfo { get; set; }

        [DataMember, JsonProperty("ActiveSmokeSessions")]
        public ICollection<SmokeSessionSimpleDto> ActiveSmokeSessions { get; set; }

        [DataMember, JsonProperty("ActiveReservations")]
        public ICollection<ReservationDto> ActiveReservations { get; set; }

        [DataMember, JsonProperty("ActiveHookahOrders")]
        public ICollection<HookahOrderDto> ActiveHookahOrders { get; set; }

        [DataMember, JsonProperty("GameProfile")]
        public GameProfileSimpleDto GameProfile { get; set; }
    }

    public class DeviceUpdateInfoDto
    {
        [DataMember, JsonProperty("StableVersion")]
        public UpdateDto StableVersion { get; set; }

        [DataMember, JsonProperty("BetaVersion")]
        public UpdateDto BetaVersion { get; set; }
    }
}