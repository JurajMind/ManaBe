using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GuigleAPI;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Models;
using smartHookah.Models.Db;
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

        public PlaceService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public async Task<Place> GetPlace(int id)
        {
            var place = await this.db.Places
                .Include(a => a.Person)
                .Include(a => a.OrderExtras)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (place == null) throw new ManaException(ErrorCodes.PlaceNotFound, "Place with id 33 not found.");
            return place;
        }

        public async Task<Place> GetManagedPlace(int id)
        {
            var place = await this.GetPlace(id);

            if (this.personService.IsPlaceManager(id)) return place;

            return null;
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

        public async Task<Place> AddPlace(Place place)
        {
            var tryFindPlace = this.db.Places.FirstOrDefault(a =>
                (a.Address.Lat == place.Address.Lat && a.Address.Lat == place.Address.Lat) || a.Name == place.Name);

            if (tryFindPlace != null)
            {
                return tryFindPlace;
            }
            
            if (place == null) throw new ArgumentNullException(nameof(place));
            place.Address = await GetLocation(place.Address);
            place.FranchiseId = await db.Franchises.AnyAsync(a => a.Id == place.FranchiseId) ? place.FranchiseId : null;
            place.PersonId = await db.Persons.AnyAsync(a => a.Id == place.PersonId) ? place.PersonId : null;
            db.Places.Add(place);
            try
            {

                await db.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);
                throw;
            }


            return place;
        }

        public async Task<Address> GetLocation(Address address)
        {
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

            if (result.Results.Any())
            {
                var firstResult = result.Results.First();
                address.Lat = firstResult.Geometry.Location.Lat.ToString(CultureInfo.InvariantCulture);
                address.Lng = firstResult.Geometry.Location.Lng.ToString(CultureInfo.InvariantCulture);
                address.Location = DbGeography.FromText($"POINT({address.Lat} {address.Lng})");
            }

            return address;
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