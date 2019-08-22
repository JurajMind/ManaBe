using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.Models.Db.Session.Dto;
using smartHookah.Models.Dto.Gear;
using smartHookah.Models.Dto.Places;
using smartHookah.Services.Review;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/Review")]
    public class ReviewController : ApiController
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }


        #region Place

        [HttpGet, Route("Place/{id}")]
        public async Task<IEnumerable<PlaceReviewDto>> GetPlaceReviews(int id, int pageSize = 10, int page = 0)
        {
            var reviews = await this.reviewService.GetPlaceReviews(id, pageSize, page);
            return PlaceReviewDto.FromModelList(reviews);
        }

        [HttpPost, Route("Place/{id}")]
        public async Task<PlaceReviewDto> AddPlaceReview([FromBody] PlaceReviewDto reviewDto)
        {
            var mReview = reviewDto.ToModel();
            var review = await this.reviewService.AddPlaceReview(mReview);
            return PlaceReviewDto.FromModel(review);
        }

        [HttpDelete, Route("Place/{id}")]
        public async Task<bool> RemovePlaceReview(int id)
        {
            return await this.reviewService.DeletePlaceReview(id);
        }

        #endregion

        #region SmokeSession

        [HttpGet, Route("Session/{id}")]
        public async Task<IEnumerable<SessionReviewDto>> GetSessionReview(int id, int pageSize = 10, int page = 0)
        {
            var reviews = await this.reviewService.GetSessionReviews(id, pageSize, page);
            return SessionReviewDto.FromModelList(reviews);
        }

        [HttpPost, Route("Session/{id}")]
        public async Task<SessionReviewDto> AddSessionReview(int id,[FromBody] SessionReviewDto reviewDto)
        {
            var mReview = reviewDto.ToModel();
            var review = await this.reviewService.AddSessionReviews(id,mReview);
            return SessionReviewDto.FromModel(review);
        }

        [HttpDelete, Route("Session/{id}")]
        public async Task<bool> RemoveSessionReview(int id)
        {
            return await this.reviewService.DeleteSessionReviews(id);
        }



        #endregion

        #region TobaccoReview

        [HttpGet, Route("Tobacco/{id}")]
        public async Task<IEnumerable<TobaccoReviewDto>> GetTobaccoReview(int id, int pageSize = 10, int page = 0)
        {
            var reviews = await this.reviewService.GetTobaccoReviews(id, pageSize, page);
            return TobaccoReviewDto.FromModelList(reviews);
        }

        [HttpPost, Route("Tobacco/{id}")]
        public async Task<TobaccoReviewDto> AddTobaccoReview([FromBody] TobaccoReviewDto reviewDto)
        {
            var mReview = reviewDto.ToModel();
            var review = await this.reviewService.AddTobaccoReviews(mReview);
            return TobaccoReviewDto.FromModel(review);
        }

        [HttpDelete, Route("Tobacco/{id}")]
        public async Task<bool> RemoveTobaccoReview(int id)
        {
            return await this.reviewService.DeleteTobaccoReviews(id);
        }

        [HttpGet, Route("Tobacco/{id}/Detail")]
        public async Task<TobaccoReviewDetailDto> TobaccoReviewDetail(int id)
        {
            var review =  await this.reviewService.GetTobaccoReviewDetail(id);

            return TobaccoReviewDetailDto.FromModel(review);
        }

        #endregion
    }
}