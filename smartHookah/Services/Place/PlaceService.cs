using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Models;

namespace smartHookah.Services.Place
{
    using smartHookah.Services.Person;

    using Place = smartHookah.Models.Place;

    public class PlaceService : IPlaceService
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public PlaceService(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        public async Task<Models.Place> GetPlace(int id)
        {
            var place = await db.Places.FirstOrDefaultAsync(a => a.Id == id);
            if (place == null) throw new ItemNotFoundException($"Place with id {id} not found.");
            return place;
        }

        public async Task<Place> GetManagedPlace(int id)
        {
            var place = await this.GetPlace(id);

            if (this.personService.IsPlaceManager(id)) return place;

            return null;
        }

        public async Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0) =>
            await db.TobaccoReviews
                .Where(a => a.SmokeSession.PlaceId != null && a.SmokeSession.PlaceId == id)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();
        
    }
}