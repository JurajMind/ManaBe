using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class SmokeSessionMetaDataDto
    {
        [DataMember, JsonProperty("Id")]
        public int Id { get; set; }

        [DataMember, JsonProperty("TobaccoId")]
        public int? TobaccoId { get; set; }

        [DataMember, JsonProperty("Tobacco")]
        public PipeAccesorySimpleDto Tobacco { get; set; }

        [DataMember, JsonProperty("TobaccoMix")]
        public TobaccoMixSimpleDto TobaccoMix { get; set; }

        [DataMember, JsonProperty("TobaccoWeight")]
        public double TobaccoWeight { get; set; }

        [DataMember, JsonProperty("AnonymPeopleCount")]
        public int AnonymPeopleCount { get; set; }

        [DataMember, JsonProperty("BowlId")]
        public int? BowlId { get; set; }

        [DataMember, JsonProperty("Bowl")]
        public PipeAccesorySimpleDto Bowl { get; set; }

        [DataMember, JsonProperty("PipeId")]
        public int? PipeId { get; set; }

        [DataMember, JsonProperty("Pipe")]
        public PipeAccesorySimpleDto Pipe { get; set; }

        [DataMember, JsonProperty("CoalId")]
        public int? CoalId { get; set; }

        [DataMember, JsonProperty("Coal")]
        public PipeAccesorySimpleDto Coal { get; set; }

        [DataMember, JsonProperty("HeatManagementId")]
        public int? HeatManagementId { get; set; }

        [DataMember, JsonProperty("HeatManagement")]
        public PipeAccesorySimpleDto HeatManagement { get; set; }

        [DataMember, JsonProperty("PackType")]
        public PackType PackType { get; set; }

        [DataMember, JsonProperty("CoalCount")]
        public double CoalsCount { get; set; }

        public static SmokeSessionMetaDataDto FromModel(SmokeSessionMetaData model) => model == null
            ? null
            : new SmokeSessionMetaDataDto()
            {
                Id = model.Id,
                TobaccoId = model.TobaccoId,
                Tobacco = model.Tobacco == null ? null : PipeAccesorySimpleDto.FromModel(model.Tobacco),
                TobaccoMix = TobaccoMixSimpleDto.FromModel(model.Tobacco as TobaccoMix,null),
                TobaccoWeight = model.TobaccoWeight,
                AnonymPeopleCount = model.AnonymPeopleCount,
                BowlId = model.BowlId,
                Bowl = model.Bowl == null ? null : PipeAccesorySimpleDto.FromModel(model.Bowl),
                PipeId = model.PipeId,
                Pipe = model.Pipe == null ? null : PipeAccesorySimpleDto.FromModel(model.Pipe),
                CoalId = model.CoalId,
                Coal = model.Coal == null ? null : PipeAccesorySimpleDto.FromModel(model.Coal),
                HeatManagementId = model.HeatManagementId,
                HeatManagement = model.HeatManagement == null ? null : PipeAccesorySimpleDto.FromModel(model.HeatManagement),
                PackType = model.PackType,
                CoalsCount = model.CoalsCount
            }; 
        
        public SmokeSessionMetaData ToModel()
        {
            return new SmokeSessionMetaData()
            {
                Id = Id, 
                TobaccoId = TobaccoId, 
                BowlId = BowlId, 
                PipeId = PipeId, 
                CoalId = CoalId, 
                HeatManagementId = HeatManagementId, 
                PackType = PackType, 
                CoalsCount = CoalsCount
            }; 
        }
    }
}