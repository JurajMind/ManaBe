using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class PipeAccesoryStatistics
    {
        public int Used { get; set; }

        public double Weight { get; set; }

        [NotMapped]
        public TimeSpan SmokeDuration
        {
            get
            {
                return TimeSpan.FromTicks(SmokeDurationTick);
            }
            set { SmokeDurationTick = value.Ticks; }
        }
       
        [Key]
        [ForeignKey("PipeAccesory")]
        public int PipeAccesoryId { get; set; }

        public virtual PipeAccesory PipeAccesory { get; set; }

        public Int64 SmokeDurationTick { get; set; }

        public double PufCount { get; set; }

        public double BlowCount { get; set; }

        [NotMapped]
        public TimeSpan SessionDuration
        {
            get
            {
                return TimeSpan.FromTicks(SessionDurationTick);
            }
            set { SessionDurationTick = value.Ticks; }
        }

        public long SessionDurationTick { get; set; }


        public PackType PackType { get; set; }

        public double Quality { get; set; }
        public double Taste { get; set; }

        public double Smoke { get; set; }

        public double Overall { get; set; }

        public double SmokeTimePercentil { get; set; }
        public double SessionTimePercentil { get; set; }

    }
}