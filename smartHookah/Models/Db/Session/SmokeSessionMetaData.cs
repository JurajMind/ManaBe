using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class SmokeSessionMetaData
    {
        public int Id { get; set; }

        [ForeignKey("Tobacco")]
        public int? TobaccoId { get; set; }
        public virtual Tobacco Tobacco { get; set; }
        public double TobaccoWeight { get; set; }
        public int AnonymPeopleCount { get; set; }

        [ForeignKey("Bowl")]
        public int? BowlId { get; set; }
        public virtual Bowl Bowl { get; set; }

        [ForeignKey("Pipe")]
        public int? PipeId { get; set; }
        public virtual Pipe Pipe { get; set; }

        [ForeignKey("Coal")]
        public int? CoalId { get; set; }
        public virtual Coal Coal { get; set; }

        [ForeignKey("HeatManagement")]
        public int? HeatManagementId { get; set; }
        public virtual HeatManagment HeatManagement { get; set; }

        public PackType PackType { get; set; }

        public HeatKeeper HeatKeeper { get; set; }

        public CoalType CoalType { get; set; }
        public double CoalsCount { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }


        public void Copy(SmokeSessionMetaData hookahDefaultMetaData)
        {
            if (hookahDefaultMetaData.BowlId != null)
                this.BowlId = hookahDefaultMetaData.BowlId;

            if (hookahDefaultMetaData.PipeId != null)
                this.PipeId = hookahDefaultMetaData.PipeId;

            if (hookahDefaultMetaData.PackType != PackType.Unknown)
                this.PackType = hookahDefaultMetaData.PackType;

            if (hookahDefaultMetaData.CoalType != CoalType.Unknown)
                this.CoalType = hookahDefaultMetaData.CoalType;

            if (hookahDefaultMetaData.HeatKeeper != HeatKeeper.Unknown)
                this.HeatKeeper = hookahDefaultMetaData.HeatKeeper;

            if (hookahDefaultMetaData.CoalsCount != 0)
                this.CoalsCount = hookahDefaultMetaData.CoalsCount;

            if (hookahDefaultMetaData.TobaccoId != null)
                this.TobaccoId = hookahDefaultMetaData.TobaccoId;
        }
    }

    public enum PackType
    {
        Unknown = 0,
        Fluffy = 1,
        SemiDense = 2,
        Dense = 3,
        OverPack = 4
    }

    public enum HeatKeeper
    {
        Unknown = 0,
        Foil = 1,
        HMS =2,
        Ignis = 3,
        Kazach = 4,
        Badcha = 5,
    }

    public enum CoalType
    {
        Unknown = 0,
        InstantLight = 1,
        Coconut = 2,
        Bamboo = 3,
    }
}