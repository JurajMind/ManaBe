using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Spatial;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GuigleAPI;
using Microsoft.VisualStudio.Services.Common;
using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Places;
using smartHookah.Services.Device;
using smartHookah.Services.Redis;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Services.Place
{
    using smartHookah.Services.Person;

    using Place = Models.Db.Place;

    public class PlaceService : IPlaceService
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        private readonly IRedisService redisService;

        private readonly IDeviceService deviceService;

        public PlaceService(SmartHookahContext db, IPersonService personService, IRedisService redisService, IDeviceService deviceService)
        {
            this.db = db;
            this.personService = personService;
            this.redisService = redisService;
            this.deviceService = deviceService;
        }

        public async Task<Place> GetPlace(int id)
        {
            var place = await this.db.Places
                .Include(a => a.Person)
                .Include(a => a.OrderExtras)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (place == null) throw new ManaException(ErrorCodes.PlaceNotFound, $"Place with id {id} not found.");
            return place;
        }

        public async Task<Place> GetManagedPlace(int id)
        {
            var place = await this.GetPlace(id);

            if (this.personService.IsPlaceManager(id)) return place;

            return null;
        }

        public async Task<bool> CanManagePlace(int id)
        {
            var result = await this.GetManagedPlace(id);
            return result != null;
        }


        public async Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0) =>
            await this.db.TobaccoReviews
                .Where(a => a.SmokeSession.PlaceId != null && a.SmokeSession.PlaceId == id)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();

        public IEnumerable<PipeAccesory> GetPlaceAccessories(Place place)
        {
            return this.db.Persons
                .FirstOrDefault(a => a.Id == place.Person.Id)?
                .OwnedPipeAccesories
                .Select(a => a.PipeAccesory);
        }


        public async Task<List<TobaccoMix>> GetPlaceTobaccoMixes(Place place)
        {
            return await this.db.TobaccoMixs.Where(a => a.AuthorId == place.Person.Id).ToListAsync();
        }

        public async Task<Place> AddPlace(Place place,List<string> flags)
        {
            var tryFindPlace = this.db.Places.FirstOrDefault(a =>
                (a.Address.Lat == place.Address.Lat && a.Address.Lat == place.Address.Lat) || a.Name == place.Name);

            if (tryFindPlace != null)
            {
                // return tryFindPlace;
            }
            
            if (place == null) throw new ArgumentNullException(nameof(place));
            place.Address = await GetLocation(place.Address);
            place.FranchiseId = await db.Franchises.AnyAsync(a => a.Id == place.FranchiseId) ? place.FranchiseId : null;
            place.PersonId = await db.Persons.AnyAsync(a => a.Id == place.PersonId) ? place.PersonId : null;
            db.Places.Add(place);
            db.SaveChanges();
            return this.AddFlags(place, flags);
        }

        public async Task<Address> GetLocation(Address address)
        {
            if (address.City != null && address.Number != null && address.Lat != null && address.Lng != null)
            {
                address.Location = GeographyExtensions.CreatePoint(address.Lat, address.Lng);
                return address;
            }
            var key = ConfigurationManager.AppSettings["googleMapApiKey"];
            GoogleGeocodingAPI.GoogleAPIKey = key;
            if (address.Street == null && (address.Lng != null && address.Lat != null))
            {
                if (!string.IsNullOrEmpty(address.ZIP) && address.ZIP.Any(a => char.IsDigit(a)))
                {
                    var posibleResult = await GoogleGeocodingAPI.SearchAddressAsync(address.ZIP);
                    if (posibleResult.Results.Any())
                    {

                        var tryAdress =  ParseGoogleResult(posibleResult.Results.First());
                        

                    }
                }

                var byCo = await GoogleGeocodingAPI.GetAddressFromCoordinatesAsync(double.Parse(address.Lat),
                    double.Parse(address.Lng));

                if (byCo.Results.Any())
                {
                    return ParseGoogleResult(byCo.Results.First());


                }
            }

            var result = await GoogleGeocodingAPI.SearchAddressAsync(address.ToString());

            if (result.Results.Any() && (this.ValidateCoordinates(address.Lat,address.Lng) ?? false ))
            {
                var firstResult = result.Results.First();
                address.Lat = firstResult.Geometry.Location.Lat.ToString(CultureInfo.InvariantCulture);
                address.Lng = firstResult.Geometry.Location.Lng.ToString(CultureInfo.InvariantCulture);
                address.Location = DbGeography.FromText($"POINT({address.Lat} {address.Lng})");
            }

            return address;
        }

        public async Task<Place> AddFlags(int placeId, List<string> flags)
        {
            if (flags == null || flags.Count == 0)
            {
                return null;
            }

            var place = this.db.Places.Find(placeId);

            if (place == null)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound,$"Place {placeId} was not found");
            }

            return AddFlags(place, flags);

        }

        private Place AddFlags(Place place, List<string> flags)
        {
            if (flags == null || flags.Count == 0)
            {
                return null;
            }

            if (place == null)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound, $"Place was not found");
            }

            var uniqFlags = place.PlaceFlags == null ? flags  : flags.Where(a => place.PlaceFlags.Count(f => f.Code == a) == 0).ToList();
            var dbFlags = this.db.PlaceFlags.Where(f => uniqFlags.Contains(f.Code)).ToList();

            if (place.PlaceFlags != null)
            {
                place.PlaceFlags.AddRange(dbFlags);
            }
            else
            {
                place.PlaceFlags = dbFlags;
            }


            this.db.Places.AddOrUpdate(place);
            this.db.SaveChanges();

            return this.db.Places.Find(place.Id);
        }

        public async Task<PlaceDashboardDto> PlaceDashboard(int placeId)
        {
            var place = await this.db.Places.FindAsync(placeId);

            var devices = place.Person.Hookahs.ToList();

            if (place == null)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound,"$Place not found");
            }

            var result = new PlaceDashboardDto();
            var deviceStatus = await this.deviceService.GetOnlineStates(devices.Select(s => s.Code));
            foreach (var device in devices)
            {
                var devicePart = new DevicePlaceDashboardDto {Device = DeviceSimpleDto.FromModel(device)};
                if (deviceStatus.TryGetValue(device.Code, out var deviceState))
                {
                    devicePart.Device.IsOnline = deviceState;
                }
                var sessionId = this.redisService.GetSessionId(device.Code);
                devicePart.Statistic = new DynamicSmokeStatisticRawDto(this.redisService.GetDynamicSmokeStatistic(sessionId));
                var session = this.db.SmokeSessions.FirstOrDefault(s => s.SessionId == sessionId);
                devicePart.MetaData = SmokeSessionMetaDataDto.FromModel(session?.MetaData);
                result.PlaceDevices.Add(devicePart);
            }

            return result;
        }

        public bool? ValidateCoordinates(double? lng, double? lat)
        {
            if (!lat.HasValue && !lng.HasValue) return null;

            var result = lng > -180 && lng <= 180 && lat >= -90 && lat <= 90;
            return result;
        }

        public async Task<List<Place>> GetWaitingPlaces()
        {
            var places = this.db.Places.Where(a => a.State == PlaceState.Waiting).ToListAsync();
            return await places;
        }

        public async Task<Place> ChangePlaceState(int placeId,PlaceState newState)
        {
            var place = await this.db.Places.FindAsync(placeId);
            if (place == null)
            {
                throw new ManaException(ErrorCodes.PlaceNotFound, $"Place was not found");
            }

            place.State = newState;
            this.db.Places.AddOrUpdate(place);
            db.SaveChanges();
            return place;
        }

        public bool? ValidateCoordinates(string lng, string lat)
        {
            if (double.TryParse(lng, out var dLng) && double.TryParse(lat, out var dLat))
            {
                return this.ValidateCoordinates(dLng, dLat);
            }

            return false;
        }

        public Address ParseGoogleResult(GuigleAPI.Model.Address googleAddress)
        {
            var zip = googleAddress.AddressComponents.Where(a => a.Types.Contains("postal_code")).Select(s => s.LongName).FirstOrDefault();
            var streed = googleAddress.AddressComponents.Where(a => a.Types.Contains("route")).Select(s => s.LongName).FirstOrDefault();
            var number = googleAddress.AddressComponents.Where(a => a.Types.Contains("street_number")).Select(s => s.LongName).FirstOrDefault();
            var city = googleAddress.AddressComponents.Where(a => a.Types.Contains("sublocality")).Select(s => s.LongName).FirstOrDefault();
            var Lat = googleAddress.Geometry.Location.Lat.ToString(CultureInfo.InvariantCulture);
            var Lng = googleAddress.Geometry.Location.Lng.ToString(CultureInfo.InvariantCulture);

            return new Address
            {
                ZIP = zip,
                City = city,
                Lat = Lat,
                Lng = Lng,
                Number = number,
                Street = streed,
                Location = DbGeography.FromText($"POINT({Lng} {Lat})")
            };

        }
    }
}