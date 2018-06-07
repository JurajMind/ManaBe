using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/Places")]
    public class PlacesController : ApiController
    {
        private readonly SmartHookahContext _db;

        public PlacesController(SmartHookahContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("SearchNearby")]
        [AcceptVerbs("GET", "POST")]
    public async Task<NearbyPlacesDTO> SearchNearby(float lng, float lat, int page = 10)
        {
            if (!ValidateCoordinates(lng, lat)) return new NearbyPlacesDTO() {Message = "Cannot find your location."};
            if (page < 0) page = 10;

            NearbyPlacesDTO result = new NearbyPlacesDTO();
            result.NearbyPlaces = new List<PlaceResult>();
            var myLocation = DbGeography.FromText($"POINT({lat} {lng})");
            var closestPlaces =
                (from u in _db.Places orderby u.Address.Location.Distance(myLocation) select u).Take(page);

            foreach (var place in closestPlaces)
            {
                PlaceResult p = new PlaceResult()
                {
                    Id = place.Id,
                    Address = place.Address,
                    FriendlyUrl = place.FriendlyUrl,
                    LogoPath = place.LogoPath,
                    Name = place.Name,
                    Rating = 0
                };
                foreach (var item in place.BusinessHours)
                {
                    var h = new OpeningDay()
                    {
                        Day = item.Day,
                        OpenTime = item.OpenTine,
                        CloseTime = item.CloseTime
                    };
                    p.BusinessHours.Add(h);
                }

                result.NearbyPlaces.Add(p);
            }

            result.Message = result.NearbyPlaces.Count > 0
                ? $"{result.NearbyPlaces.Count} places found nearby."
                : "No places nearby.";

            return result;
        }

        private bool ValidateCoordinates(float lng, float lat)
        {
            bool result = (lng > -180 && lng <= 180 && lat >= -90 && lat <= 90);
            return result;
        }
    }
}