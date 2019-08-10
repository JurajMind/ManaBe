using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session;

namespace smartHookah.Services.Review
{
    public interface IReviewService
    {
        // PlaceReview
        Task<IEnumerable<PlaceReview>> GetPlaceReviews(int id, int pageSize = 10, int page = 0);

        Task<PlaceReview> AddPlaceReview(PlaceReview review);

        Task<bool> DeletePlaceReview(int id);

        // AccessoryReview
        Task<IEnumerable<PipeAccessoryReview>> GetPipeAccessoryReviews(int id, int pageSize = 10, int page = 0);

        Task<PipeAccessoryReview> AddPipeAccessoryReviews(PipeAccessoryReview review);

        Task<bool> DeletePipeAccessoryReview(int id);

        //TobaccoReview
        Task<IEnumerable<TobaccoReview>> GetTobaccoReviews(int id, int pageSize = 10, int page = 0);

        Task<TobaccoReview> AddTobaccoReviews(TobaccoReview review);

        Task<bool> DeleteTobaccoReviews(int id);

        //SessionReview
        Task<IEnumerable<SessionReview>> GetSessionReviews(int id, int pageSize = 10, int page = 0);

        Task<SessionReview> AddSessionReviews(SessionReview review);

        Task<bool> DeleteSessionReviews(int id);

    }
}