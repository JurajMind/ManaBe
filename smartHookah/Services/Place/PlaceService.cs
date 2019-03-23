using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Spatial;
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
            if (place == null) throw new ArgumentNullException(nameof(place));
            place.Address = await GetLocation(place.Address);
            place.FranchiseId = await db.Franchises.AnyAsync(a => a.Id == place.FranchiseId) ? place.FranchiseId : null;
            place.PersonId = await db.Persons.AnyAsync(a => a.Id == place.PersonId) ? place.PersonId : null;
            db.Places.Add(place);
            await db.SaveChangesAsync();

            return place;
        }

        public async Task<Address> GetLocation(Address address)
        {
            var key = ConfigurationManager.AppSettings["googleMapApiKey"];
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
    }
}