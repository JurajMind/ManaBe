namespace smartHookah.Controllers.Api
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using smartHookah.Models;
    using smartHookah.Models.Dto;

    [RoutePrefix("api/Places")]
    public class PlacesController : ApiController
    {
        private readonly SmartHookahContext _db;

        public PlacesController(SmartHookahContext db)
        {
            this._db = db;
        }

        [HttpGet]
        [Route("SearchNearby")]
        public async Task<NearbyPlacesDTO> SearchNearby(float? lng = null, float? lat=null, int page = 10)
        {
            var validate = this.ValidateCoordinates(lng, lat);
            if (validate.HasValue && !validate.Value)
                return new NearbyPlacesDTO { Message = "Cannot find your location." };
            if (page < 0) page = 10;

            var result = new NearbyPlacesDTO();
            result.NearbyPlaces = new List<PlaceResult>();

            IQueryable<Place> closestPlaces;
            var places = this._db.Places.Include("BusinessHours");
            if (validate.HasValue)
            {
                var myLocation = DbGeography.FromText($"POINT({lat} {lng})");

                closestPlaces = (from u in places orderby u.Address.Location.Distance(myLocation) select u).Take(page);
            }
            else
            {
                closestPlaces = places.OrderBy(a => a.Id).Take(page);
            }

            foreach (var place in closestPlaces)
            {
                var p = new PlaceResult
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
                    var h = new OpeningDay { Day = item.Day, OpenTime = item.OpenTine, CloseTime = item.CloseTime };
                    p.BusinessHours.Add(h);
                }

                result.NearbyPlaces.Add(p);
            }

            result.Message = result.NearbyPlaces.Count > 0
                                 ? $"{result.NearbyPlaces.Count} places found nearby."
                                 : "No places nearby.";

            return result;
        }

        private bool? ValidateCoordinates(float? lng, float? lat)
        {
            if (!lat.HasValue && !lng.HasValue) return null;

            var result = lng > -180 && lng <= 180 && lat >= -90 && lat <= 90;
            return result;
        }
    }
}