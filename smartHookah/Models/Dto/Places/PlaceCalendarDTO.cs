using System;
using System.Collections.Generic;

namespace smartHookah.Models.Dto
{
    public class PlaceCalendarDto
    {
        public ICollection<PlaceDay> PlaceDays { get; set; }

        public PlaceCalendarDto()
        {
            this.PlaceDays = new List<PlaceDay>();
        }
    }

    public class PlaceDay
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public DateTime Day { get; set; }
        public TimeSpan OpenHour { get; set; }
        public TimeSpan CloseHour { get; set; }
        public ICollection<PlaceEventDTO> PlaceEvents { get; set; }

        public PlaceDay()
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