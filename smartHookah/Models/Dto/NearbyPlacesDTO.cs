using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class NearbyPlacesDTO : DTO
    {
        public ICollection<PlaceSimpleDTO> NearbyPlaces { get; set; }
    }

    public class OpeningDay
    {
        public int Day { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }

    public class PlaceSimpleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FriendlyUrl { get; set; }
        public string LogoPath { get; set; }
        public Address Address { get; set; }
        public ICollection<OpeningDay> BusinessHours { get; set; }
        public int Rating { get; set; }

        public PlaceSimpleDTO()
        {
            Address = new Address();
            BusinessHours = new List<OpeningDay>();
        }

        public static PlaceSimpleDTO FromModel(Place model)
        {
            return new PlaceSimpleDTO
                       {
                           Id = model.Id,
                           Name = model.Name,
                           FriendlyUrl = model.FriendlyUrl,
                           LogoPath = model.LogoPath,
                           Address = model.Address,
                           //BusinessHours = model.BusinessHours.ToList(),
                           //Rating = model.
                       };
        }
    }
}