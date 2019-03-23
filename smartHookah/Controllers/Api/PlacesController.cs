using System.Collections;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Services.Common;
using smartHookah.ErrorHandler;
using smartHookah.Models.Db;
using smartHookahCommon.Extensions;

namespace smartHookah.Controllers.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
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

        [HttpGet, Route("{id}/Menu")]
        public async Task<PlaceMenuDto> GetPlaceMenu(int id)
        {
           
                var place = await placeService.GetPlace(id);
                var accessories = placeService.GetPlaceAccessories(place);
                var mixes = await placeService.GetPlaceTobaccoMixes(place);


                var priceGroups = place.PriceGroups.ToList().Select(a => new PriceGroupDto(a)).ToList();

                var priceMatrix = new Dictionary<int, Dictionary<int, decimal>>();

                foreach (var pc in priceGroups)
                {
                    var pcMatrix = new Dictionary<int, decimal>();
                    foreach (var item in place.Person.OwnedPipeAccesories)
                    {
                        var priceGroup = item.Prices.FirstOrDefault(a => a.PriceGroupId == pc.Id);
                        if (priceGroup != null) pcMatrix.Add(item.PipeAccesoryId, priceGroup.Price);
                    }

                    priceMatrix.Add(pc.Id, pcMatrix);
                }

                return new PlaceMenuDto()
                {
                    OrderExtras = OrderExtraDto.FromModelList(place.OrderExtras).ToList(),
                    Accessories = PipeAccesorySimpleDto.FromModelList(accessories).ToList(),
                    TobaccoMixes = TobaccoMixSimpleDto.FromModelList(mixes).ToList(),
                    PriceGroup = priceGroups,
                    BasePrice = place.BaseHookahPrice,
                    PriceMatrix = priceMatrix.Select(s => new PriceGroupItems{GroupId = s.Key,Prices = s.Value}).ToList(),
                    Currency = place.Currency
                };
            }

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
        [Route("FixLocation")]
        public void FixLocation()
        {
            var addresses = this.db.Addresses.ToList();
            foreach (var address in addresses)
            {
                if(address.Lat == null)
                    continue;

               var location =  DbGeography.FromText($"POINT({address.Lat} {address.Lng})");
               address.Location = location;
               this.db.Addresses.AddOrUpdate(address);
            }

            this.db.SaveChanges();

        }

        [HttpGet]
        [Route("SearchNearby")]
        public async Task<NearbyPlacesDto> SearchNearby(int page = 0, int pageSize = 10, float? lat = null, float? lng = null)
        {
            var validate = this.ValidateCoordinates(lng, lat);
            if (validate.HasValue && !validate.Value)
                return new NearbyPlacesDto {Success = false, Message = "Cannot find your location."};
            if (pageSize < 0) pageSize = 10;

            var result = new NearbyPlacesDto();
            

            IQueryable<Place> closestPlaces;
            var places = this.db.Places.Include("BusinessHours").Where(a => a.Public);
            if (validate.HasValue)
            {
                var myLocation = DbGeography.FromText($"POINT({lat} {lng})");

                closestPlaces = this.db.Places.OrderBy(a => a.Address.Location.Distance(myLocation))
                    .Skip(page * pageSize).Take(pageSize);
            }
            else
            {
                closestPlaces = places.OrderBy(a => a.Id).Skip(pageSize * page).Take(pageSize);
            }

            result.NearbyPlaces = PlaceSimpleDto.FromModelList(closestPlaces.ToList()).ToList();
          

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

        #region Setters
        
        [HttpPost, Route("Import")]
        public async Task<IHttpActionResult> ImportPlaces()
        {
            var stream = await Request.Content.ReadAsStreamAsync();
            
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.TrimOptions = TrimOptions.Trim;
                
                var records = csv.GetRecords<PlaceImportModel>();

                foreach (var record in records)
                {
                    var place = PlaceImportModel.ToModel(record);
                    await placeService.AddPlace(place);
                }
            }

            return Ok();
        }

        [HttpPost, Route("ImportMap")]
        public async Task<IHttpActionResult> ImportPlacesFromMap()
        {
            var stream = await Request.Content.ReadAsStreamAsync();

            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.TrimOptions = TrimOptions.Trim;

                var records = csv.GetRecords<PlaceImportModelMap>();

                foreach (var record in records)
                {
                    var place = PlaceImportModelMapToPlace(record);
                    await placeService.AddPlace(place);
                }
            }

            return Ok();
        }

        public  Place PlaceImportModelMapToPlace (PlaceImportModelMap model)
        {

            return new Place()
            {
                Name = model.Name,
                FriendlyUrl = getFriendlyUr(model.Name),
                Address = new Address()
                {
                    Lat = model.Lat,
                    Lng = model.Lng,
                    ZIP = model.PosibleAdress,
                },
                Facebook = model.Url
            };
        }

        public string getFriendlyUr(string name)
        {

            var clean = name.RemoveDiacritics();
            var friendlyUrl = string.Concat(clean.ToLower().Replace(' ', '_').Where(char.IsLetterOrDigit));
            var match = this.db.Places.FirstOrDefault(a => a.FriendlyUrl == friendlyUrl);
            if (match != null)
            {
                var count = 1;
                friendlyUrl = $"{friendlyUrl}_{count.ToString()}";
                while (match != null)
                {
                    match = this.db.Places.FirstOrDefault(a => a.FriendlyUrl == friendlyUrl);
                    count++;
                }
              
            }

            if (friendlyUrl.Length > 25)
            {
                return friendlyUrl.Substring(0, 25);
            }
            return friendlyUrl;
        }

        #endregion

    }
}