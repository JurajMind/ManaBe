using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace smartHookah.Models.Db.Dto
{
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
        [JsonProperty("defaut")]
        public bool Defaut { get; set; }

        [DataMember]
        [JsonProperty("personId")]
        public int? PersonId { get; set; }

        [DataMember]
        [JsonProperty("settingId")]
        public int SettingId { get; set; }

        public static DevicePresetDto FromModel(DevicePreset model) => model == null
            ? null
            : new DevicePresetDto()
            {
                Id = model.Id,
                Name = model.Name,
                Defaut = model.Defaut,
                PersonId = model.PersonId,
                SettingId = model.SettingId,
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
                Id = Id, 
                Name = Name, 
                Defaut = Defaut, 
                PersonId = PersonId, 
                SettingId = SettingId, 
            }; 
        }
    }
}