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
    public class PlaceService : IPlaceService
    {
        private readonly SmartHookahContext db;

        public PlaceService(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<Models.Place> GetPlace(int id)
        {
            var place = await db.Places.FirstOrDefaultAsync(a => a.Id == id);
            if (place == null) throw new ItemNotFoundException($"Place with id {id} not found.");
            return place;
        }

        public async Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0) =>
            await db.TobaccoReviews
                .Where(a => a.SmokeSession.PlaceId != null && a.SmokeSession.PlaceId == id)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();
        
    }
}