using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Controllers.Api
{
    public class TobaccoReviewsController : ApiController
    {
        private readonly SmartHookahContext db;

        public TobaccoReviewsController(SmartHookahContext db)
        {
            this.db = db;
        }

        [System.Web.Mvc.HttpGet]
        public async Task<TobaccoReviewDTO> GetReviewVue(int sessionId)
        {
            var review = (sessionId < 0) ? null : db.TobaccoReviews.FirstOrDefault(a => a.SmokeSession.Id == sessionId);

            return (review != null) ? new TobaccoReviewDTO()
            {
                Id = review.Id,
                Overall = review.Overall,
                Quality = review.Quality,
                Smoke = review.Smoke,
                SmokeSessionId = review.SmokeSession.Id,
                Taste = review.Taste,
                Text = review.Text
            } : null;
        }

        
        [System.Web.Mvc.HttpPost]
        public async Task<TobaccoReviewDTO> SaveVueReview([Bind(Include = "Id,Quality,Taste,Smoke,Overall,Text,SmokeSessionId")] TobaccoReviewDTO tobaccoReviewDto)
        {
            if (tobaccoReviewDto != null)
            {
                var tobaccoReview = new TobaccoReview()
                {
                    Id = tobaccoReviewDto.Id,
                    Overall = tobaccoReviewDto.Overall,
                    Quality = tobaccoReviewDto.Quality,
                    Smoke = tobaccoReviewDto.Smoke,
                    SmokeSessionId = tobaccoReviewDto.SmokeSessionId,
                    Taste = tobaccoReviewDto.Taste,
                    Text = tobaccoReviewDto.Text
                };
                var smokeSession = await db.SmokeSessions.FindAsync(tobaccoReview.SmokeSessionId);
                tobaccoReview.PublishDate = DateTime.UtcNow;
                if (smokeSession.MetaData.TobaccoId.HasValue)
                    tobaccoReview.ReviewedTobaccoId = smokeSession.MetaData.TobaccoId.Value;
                else
                {
                    return new TobaccoReviewDTO(){ Success = false, Message = "Please fill tobacco information first" };
                }

                if (UserHelper.GetCurentPerson() == null)
                {
                    return new TobaccoReviewDTO(){ Success = false, Message = "Please log in first" };
                }

                tobaccoReview.AuthorId = UserHelper.GetCurentPerson().Id;
                
                var rev = db.TobaccoReviews.Find(tobaccoReview.Id);

                if (rev != null)
                {
                    rev.PublishDate = tobaccoReview.PublishDate;
                    rev.Quality = tobaccoReview.Quality;
                    rev.Smoke = tobaccoReview.Smoke;
                    rev.Overall = tobaccoReview.Overall;
                    rev.Text = tobaccoReview.Text;
                    rev.Taste = tobaccoReview.Taste;
                    db.Entry(rev).State = EntityState.Modified;
                    smokeSession.Review = rev;
                    db.SmokeSessions.AddOrUpdate(smokeSession);

                    await db.SaveChangesAsync();
                    return new TobaccoReviewDTO()
                    {
                        Id = rev.Id,
                        Overall = rev.Overall,
                        Quality = rev.Quality,
                        Smoke = rev.Smoke,
                        SmokeSessionId = rev.SmokeSession.Id,
                        Taste = rev.Taste,
                        Text = rev.Text
                    };
                }

                db.TobaccoReviews.AddOrUpdate(tobaccoReview);
                smokeSession.Review = tobaccoReview;
                db.SmokeSessions.AddOrUpdate(smokeSession);

                await db.SaveChangesAsync();
                return new TobaccoReviewDTO()
                {
                    Id = tobaccoReview.Id,
                    Overall = tobaccoReview.Overall,
                    Quality = tobaccoReview.Quality,
                    Smoke = tobaccoReview.Smoke,
                    SmokeSessionId = tobaccoReview.SmokeSession.Id,
                    Taste = tobaccoReview.Taste,
                    Text = tobaccoReview.Text
                };
            }
            return null;
        }
    }
}
