using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class DeviceSimpleDto
    {
        [DataMember, JsonProperty("Id")]
        public int Id { get; set; }
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
                Id = model.Id,
                Code = model.Code,
                Name = model.Name,
                IsOnline = model.OnlineState,
                Version = model.Version,
                Type = model.Type

            };
    }
}