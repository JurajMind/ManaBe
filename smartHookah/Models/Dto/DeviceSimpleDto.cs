using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace smartHookah.Models.Dto
{
    using System.Collections.Generic;

    [DataContract]
    public class DeviceSimpleDto
    {
        [DataMember, JsonProperty("Name")]
        public string Name { get; set; }

        [DataMember, JsonProperty("Code")]
        public string Code { get; set; }

        [DataMember, JsonProperty("IsOnline")]
        public bool IsOnline { get; set; }

        [DataMember, JsonProperty("Type")]
        public StandType Type { get; set; }

        [DataMember, JsonProperty("Version")]
        public int Version { get; set; }

        public static DeviceSimpleDto FromModel(Hookah model) => model == null
            ? null
            : new DeviceSimpleDto()
            {
                Code = model.Code,
                Name = model.Name,
                IsOnline = model.OnlineState,
                Version = model.Version,
                Type = model.Type

            };
    }
}