using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
    }
}