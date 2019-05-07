using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    public class PersonStatistic
    {
        [ForeignKey("Person")]
        public int Id { get; set; }
        public virtual Person Person { get; set; }

        public decimal TotalPufCount { get; set; }

        public decimal AvgPufCount { get; set; }

       public Int64 AvgSessionDurationTick { get; set; }

        [NotMapped]
        public TimeSpan AvgSessionDuration
        {
            get => TimeSpan.FromTicks(AvgSessionDurationTick);
            set => AvgSessionDurationTick = value.Ticks;
        }

        public Int64 TotalDurationTick { get; set; }

        [NotMapped]
        public TimeSpan TotalSessionDuration
        {
            get => TimeSpan.FromTicks(TotalDurationTick);
            set => TotalDurationTick = value.Ticks;
        }

        public Int64 AvgSmokeTimeTick { get; set; }

        [NotMapped]
        public TimeSpan AvgSmokeTime
        {
            get => TimeSpan.FromTicks(AvgSmokeTimeTick);
            set => AvgSmokeTimeTick = value.Ticks;
        }

        public Int64 TotalSmokeTimeTick { get; set; }

        [NotMapped]
        public TimeSpan TotalSmokeTime
        {
            get => TimeSpan.FromTicks(TotalSmokeTimeTick);
            set => TotalSmokeTimeTick = value.Ticks;
        }

    }
}