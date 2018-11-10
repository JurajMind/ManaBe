using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models;

namespace smartHookah.Services.Place
{
    public interface IPlaceService
    {
        Task<Models.Place> GetPlace(int id);
        Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0);
    }
}