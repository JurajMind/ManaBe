using System;
using System.Collections.Generic;
using System.Linq;
using smartHookah.Support;

namespace smartHookah.Models.Db
{
    public class SmokeSessionStatistics
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public TimeSpan SmokeDuration { get; set; }

        public TimeSpan LongestPuf { get; set; }

        public int PufCount { get; set; }

        public int EstimatedPersonCount { get; set; } = 0;

        public TimeSpan SessionDuration => End - Start;

        public SmokeSessionStatistics()
        { }
        public SmokeSessionStatistics(IEnumerable<Puf> pufs)
        {
            if(!pufs.Any())
                return;

            var pufsOrder = pufs.OrderBy(a => a.DateTime);
            Start = pufsOrder.First().DateTime;
            End = pufsOrder.Last().DateTime;

            var pufDurations = pufs.ToList().GetDuration(a => a.Type == PufType.In);

            var timeSpans = pufDurations as IList<TimeSpan> ?? pufDurations.ToList();
            LongestPuf = timeSpans.Max();

            if (LongestPuf > new TimeSpan(23, 0, 0))
                LongestPuf = new TimeSpan(0, 0, 0);

            PufCount = pufsOrder.Count(a => a.Type == PufType.In);

            SmokeDuration = timeSpans.Aggregate((a, b) => a + b);

            if (SmokeDuration > new TimeSpan(23, 0, 0))
                SmokeDuration = new TimeSpan(0, 0, 0);
        }
    }
}