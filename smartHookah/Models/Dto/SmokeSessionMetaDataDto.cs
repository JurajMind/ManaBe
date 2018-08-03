using System;

namespace smartHookah.Models.Dto
{
    public class SmokeSessionMetaDataDto
    {
        public int Id { get; set; }

        public int? TobaccoId { get; set; }

        public TobaccoSimpleDto TobaccoSimple { get; set; }

        public double TobaccoWeight { get; set; }

        public int AnonymPeopleCount { get; set; }

        public int? BowlId { get; set; }

        public PipeAccesorySimpleDto Bowl { get; set; }

        public int? PipeId { get; set; }

        public PipeAccesorySimpleDto Pipe { get; set; }

        public PackType PackType { get; set; }

        public HeatKeeper HeatKeeper { get; set; }

        public CoalType CoalType { get; set; }

        public double CoalsCount { get; set; }

        public static SmokeSessionMetaDataDto FromModel(SmokeSessionMetaData model)
        {
            return new SmokeSessionMetaDataDto()
            {
                Id = model.Id, 
                TobaccoId = model.TobaccoId, 
                TobaccoSimple = TobaccoSimpleDto.FromModel(model.Tobacco), 
                TobaccoWeight = model.TobaccoWeight, 
                AnonymPeopleCount = model.AnonymPeopleCount, 
                BowlId = model.BowlId, 
                Bowl = PipeAccesorySimpleDto.FromModel(model.Bowl), 
                PipeId = model.PipeId, 
                Pipe = PipeAccesorySimpleDto.FromModel(model.Pipe), 
                PackType = model.PackType, 
                HeatKeeper = model.HeatKeeper, 
                CoalType = model.CoalType, 
                CoalsCount = model.CoalsCount, 
            }; 
        }
    }
}