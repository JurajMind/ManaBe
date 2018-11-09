namespace smartHookah.Models.Dto
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using smartHookah.Models.Db;

    [DataContract]
    public class DevicePresetDto
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }

        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("personId")]
        public int? PersonId { get; set; }

        public static DevicePresetDto FromModel(DevicePreset model) => model == null
            ? null
            : new DevicePresetDto()
            {
                Id = model.Id,
                Name = model.Name,
                PersonId = model.PersonId,
            };

        public static IEnumerable<DevicePresetDto> FromModelList(ICollection<DevicePreset> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public DevicePreset ToModel()
        {
            return new DevicePreset()
            {
                Id = this.Id, 
                Name = this.Name,
                PersonId = this.PersonId, 
            }; 
        }
    }
}