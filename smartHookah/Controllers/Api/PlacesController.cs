using System.Net;
using System.Net.Http;
using ClosedXML.Excel;
using smartHookah.Models.Db;

namespace smartHookah.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using smartHookah.Models;
    using smartHookah.Models.Dto;
    using smartHookah.Services.Place;

    [RoutePrefix("api/Places")]
    public class PlacesController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IReservationService reservationService;

        private readonly IPlaceService placeService;

        public PlacesController(SmartHookahContext db, IReservationService reservationService, IPlaceService placeService)
        {
            this.db = db;
            this.reservationService = reservationService;
            this.placeService = placeService;
        }


        #region Getters

        [HttpGet, Route("GetPlaceInfo")]
        public async Task<PlaceDto> GetPlaceInfo(int id)
        {
            try
            {
                var place = await placeService.GetPlace(id);
                var reviews = await placeService.GetPlaceTobaccoReviews(id);

                var result = PlaceDto.FromModel(place);
                result.TobaccoReviews = TobaccoReviewDto.FromModelList(reviews);

                return result;
            }
            catch(Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }

        [HttpGet]
        [Route("SearchNearby")]
        public async Task<NearbyPlacesDto> SearchNearby(int page = 0, int pageSize = 10, float? lng = null, float? lat = null)
        {
            var validate = this.ValidateCoordinates(lng, lat);
            if (validate.HasValue && !validate.Value)
                return new NearbyPlacesDto {Success = false, Message = "Cannot find your location."};
            if (pageSize < 0) pageSize = 10;

            var result = new NearbyPlacesDto();
            result.NearbyPlaces = new List<PlaceSimpleDto>();

            IQueryable<Place> closestPlaces;
            var places = this.db.Places.Include("BusinessHours").Where(a => a.Public);
            if (validate.HasValue)
            {
                var myLocation = DbGeography.FromText($"POINT({lng} {lat})");

                closestPlaces = (from u in places orderby u.Address.Location.Distance(myLocation) select u).Skip(pageSize * page).Take(pageSize);
            }
            else
            {
                closestPlaces = places.OrderBy(a => a.Id).Skip(pageSize * page).Take(pageSize);
            }

            foreach (var place in closestPlaces)
            {
                var p = new PlaceSimpleDto
                {
                    Id = place.Id,
                    Address = place.Address,
                    FriendlyUrl = place.FriendlyUrl,
                    LogoPath = place.LogoPath,
                    Name = place.Name
                };
                foreach (var item in place.PlaceDays)
                {
                    var h = new OpeningDay
                    {
                        Day = (int) item.Day.DayOfWeek,
                        OpenTime = item.OpenHour,
                        CloseTime = item.CloseHour
                    };
                    p.BusinessHours.Add(h);
                }

                foreach (var media in place.Medias)
                {
                    p.Medias.Add(MediaDto.FromModel(media));
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

        #endregion

        #region Reservations
            


        #endregion

    }
}