using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Db
{
    public class PlaceDay
    {
        public int Id { get; set; }
        public DateTime Day { get; set; }
        public TimeSpan OpenHour { get; set; }
        public TimeSpan CloseHour { get; set; }
        public virtual ICollection<PlaceEvent> PlaceEvents { get; set; }
        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        public PlaceDay()
        {
            PlaceEvents = new List<PlaceEvent>();
        }
    }

    public class PlaceEvent
    {
        public int  Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string PrivacyType { get; set; }
        public string FacebookUrl { get; set; }
        public virtual ICollection<Person> Persons { get; set; }
        public int PlaceDayId { get; set; }
        public virtual PlaceDay PlaceDay { get; set; }
    }
}