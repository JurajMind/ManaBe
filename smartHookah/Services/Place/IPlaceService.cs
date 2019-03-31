using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models.Db;
using smartHookah.Models.Dto.Places;

namespace smartHookah.Services.Place
{
    using Place = Models.Db.Place;

    public interface IPlaceService
    {
        Task<Place> GetPlace(int id);
    
        Task<Place> GetManagedPlace(int id);

        Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0);

        IEnumerable<PipeAccesory> GetPlaceAccessories(Place place);

        Task<List<TobaccoMix>> GetPlaceTobaccoMixes(Place place);

        Task<Place> AddPlace(Place place);

        Task<Address> GetLocation(Address address);
        Task<Place> AddFlags(int placeId, List<string> flags);

        Task<PlaceDashboardDto> PlaceDashboard(int placeId);
    }
}