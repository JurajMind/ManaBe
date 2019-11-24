using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class InitDataDto
    {
        [DataMember, JsonProperty("SmokeSession")]
        public SmokeSessionSimpleDto SmokeSession { get; set; }

        [DataMember, JsonProperty("DeviceSettings")]
        public DeviceSettingDto DeviceSettings { get; set; }
    }
}