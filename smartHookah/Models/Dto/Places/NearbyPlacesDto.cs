using System;
using System.Collections.Generic;
using System.Linq;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;

namespace smartHookah.Models.Dto
{
    public class NearbyPlacesDto : DTO
    {
        public ICollection<PlaceSimpleDto> NearbyPlaces { get; set; }

        public NearbyPlacesDto()
        {
            NearbyPlaces = new List<PlaceSimpleDto>();
        }
    }

    public class OpeningDay
    {
        public int Day { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
    }

    public class PlaceSimpleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortDescriptions { get; set; }
        public string FriendlyUrl { get; set; }
        public string LogoPath { get; set; }
        public AddressDto Address { get; set; }
        public ICollection<BusinessHoursDto> BusinessHours { get; set; }
        public int Rating { get; set; }
        public MediaDto Media { get; set; }

        public string PhoneNumber { get; set; }
        public string Facebook { get; set; }

        public bool HaveMenu { get; set; }

        public bool HaveOrders { get; set; }

        public bool HaveMana { get; set; }

        public bool HaveReservation { get; set; }

        public PlaceSimpleDto()
        {
            Address = new AddressDto();
            BusinessHours = new List<BusinessHoursDto>();
            Media = new MediaDto();
        }

        public static PlaceSimpleDto FromModel(Place model) => model == null ? null : new PlaceSimpleDto
        {
            Id = model.Id,
            Name = model.Name,
            ShortDescriptions = model.ShortDescriptions,
            FriendlyUrl = model.FriendlyUrl,
            LogoPath = model.LogoPath,
            Address = AddressDto.FromModel(model.Address),
            Media = MediaDto.FromModelList(model.Medias).FirstOrDefault(),
            PhoneNumber = model.PhoneNumber,
            Facebook = model.Facebook,
            BusinessHours = BusinessHoursDto.FromModelList(model.BusinessHours).ToList(),
            HaveMenu = model.HaveMenu,
            HaveOrders = model.HaveOrders,
            HaveMana = model.HaveMana,
            HaveReservation = model.HaveReservation,
            Rating = model.Rating
        };

        public static IEnumerable<PlaceSimpleDto> FromModelList(ICollection<Place> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }
    }
}