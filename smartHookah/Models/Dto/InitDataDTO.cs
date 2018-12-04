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
    public class InitDataDto 
    {
        [DataMember, JsonProperty("SmokeSession")]
        public SmokeSessionSimpleDto SmokeSession { get; set; }

        [DataMember, JsonProperty("DeviceSettings")]
        public DeviceSettingDto DeviceSettings { get; set; }
    }
}