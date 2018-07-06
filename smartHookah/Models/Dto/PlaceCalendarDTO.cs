using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    public class PlaceCalendarDTO : DTO
    {
        public ICollection<PlaceDayDTO> PlaceDays { get; set; }

        public PlaceCalendarDTO()
        {
            this.PlaceDays = new List<PlaceDayDTO>();
        }
    }

    public class PlaceDayDTO : DTO
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public DateTime Day { get; set; }
        public TimeSpan OpenHour { get; set; }
        public TimeSpan CloseHour { get; set; }
        public ICollection<PlaceEventDTO> PlaceEvents { get; set; }

        public PlaceDayDTO()
        {
            this.PlaceEvents = new List<PlaceEventDTO>();
        }
    }

    public class PlaceEventDTO : DTO
    {
        public int Id { get; set; }
        public int PlaceDayId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string PrivacyType { get; set; }
        public string FacebookUrl { get; set; }
    }

    public class PlaceEventCollectionDTO : DTO
    {
        public ICollection<PlaceEventDTO> EventCollection { get; set; }

        public PlaceEventCollectionDTO()
        {
            this.EventCollection = new List<PlaceEventDTO>();
        }
    }
}