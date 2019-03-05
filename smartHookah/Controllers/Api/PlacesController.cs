using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Services.Common;
using smartHookah.ErrorHandler;
using smartHookah.Models.Db;
using ServiceStack.Common;
using HttpContext = System.Web.HttpContext;

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
        [Route("SearchNearby")]
        public async Task<NearbyPlacesDto> SearchNearby(int page = 0, int pageSize = 10, float? lng = null, float? lat = null)
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
                var myLocation = DbGeography.FromText($"POINT({lng} {lat})");

                closestPlaces = (from u in places orderby u.Address.Location.Distance(myLocation) select u).Skip(pageSize * page).Take(pageSize);
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

        #endregion

    }

    public class 
        PlaceImportModel
    {
        #region Properties

        [Index(0)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Index(1)]
        public string LogoPath { get; set; }

        [Index(2)]
        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        [Index(3)]
        public string Descriptions { get; set; }

        [Index(4)]
        [MaxLength(25)]
        public string FriendlyUrl { get; set; }

        [Index(5)]
        public string PersonId { get; set; }

        [Index(6)]
        public string PhoneNumber { get; set; }

        [Index(7)]
        public string Facebook { get; set; }

        [Index(8)]
        public string FranchiseId { get; set; }

        #region Address

        [Index(9)]
        public string Street { get; set; }

        [Index(10)]
        public string City { get; set; }

        [Index(11)]
        public string Number { get; set; }

        [Index(12)]
        public string ZIP { get; set; }

        #endregion

        #region OpeningHours

        [Index(13)]
        public string MonOpen { get; set; }

        [Index(14)]
        public string MonClose { get; set; }

        [Index(15)]
        public string TueOpen { get; set; }

        [Index(16)]
        public string TueClose { get; set; }

        [Index(17)]
        public string WedOpen { get; set; }

        [Index(18)]
        public string WedClose { get; set; }

        [Index(19)]
        public string ThuOpen { get; set; }

        [Index(20)]
        public string ThuClose { get; set; }

        [Index(21)]
        public string FriOpen { get; set; }

        [Index(22)]
        public string FriClose { get; set; }

        [Index(23)]
        public string SatOpen { get; set; }

        [Index(24)]
        public string SatClose { get; set; }

        [Index(25)]
        public string SunOpen { get; set; }

        [Index(26)]
        public string SunClose { get; set; }

        #endregion        

        #endregion

        public static Place ToModel(PlaceImportModel model)
        {
            var address = new Address()
            {
                City = model.City,
                Number = model.Number,
                Street = model.Street,
                ZIP = model.ZIP
            };

            var hours = new Collection<BusinessHours>
            {
                new BusinessHours()
                {
                    Day = 0,
                    OpenTine = ParseTime(model.SunOpen),
                    CloseTime = ParseTime(model.SunClose)
                },

                new BusinessHours()
                {
                    Day = 1,
                    OpenTine = ParseTime(model.MonOpen),
                    CloseTime = ParseTime(model.MonClose)
                },

                new BusinessHours()
                {
                    Day = 2,
                    OpenTine = ParseTime(model.TueOpen),
                    CloseTime = ParseTime(model.TueClose)
                },

                new BusinessHours()
                {
                    Day = 3,
                    OpenTine = ParseTime(model.WedOpen),
                    CloseTime = ParseTime(model.WedClose)
                },

                new BusinessHours()
                {
                    Day = 4,
                    OpenTine = ParseTime(model.ThuOpen),
                    CloseTime = ParseTime(model.ThuClose)
                },

                new BusinessHours()
                {
                    Day = 5,
                    OpenTine = ParseTime(model.FriOpen),
                    CloseTime = ParseTime(model.FriClose)
                },

                new BusinessHours()
                {
                    Day = 6,
                    OpenTine = ParseTime(model.SatOpen),
                    CloseTime = ParseTime(model.SatOpen)
                }
            };

            if (Uri.TryCreate(model.LogoPath, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                using (var client = new WebClient())
                {
                    var extension = Path.GetExtension(uriResult.ToString());
                    if (!string.IsNullOrEmpty(extension))
                    {
                        const string path = "/Content/PlacePictures/";
                        var filePath = HttpContext.Current.Server.MapPath($"{path}{model.FriendlyUrl}{extension}");
                        client.DownloadFile(uriResult, filePath);
                        model.LogoPath = $"{path}{model.FriendlyUrl}{extension}";
                    }
                    else
                    {
                        model.LogoPath = "";
                    }
                    
                }
                
                
            }
            var result = new Place()
            {
                Address = address,
                AllowReservation = false,
                BusinessHours = hours,
                Facebook = model.Facebook,
                FranchiseId = int.Parse(model.FranchiseId),
                PersonId = int.Parse(model.PersonId),
                Descriptions = model.Descriptions,
                ShortDescriptions = model.ShortDescriptions,
                LogoPath = model.LogoPath,
                Name = model.Name,
                FriendlyUrl = model.FriendlyUrl,
                PhoneNumber = model.PhoneNumber,
                Public = false
            };
            return result;
        }

        private static TimeSpan ParseTime(string input) => input.IsNullOrEmpty()
            ? TimeSpan.Zero
            : TimeSpan.Parse(input);
    }
}