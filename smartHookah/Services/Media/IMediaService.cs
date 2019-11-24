using System.Threading.Tasks;
using System.Web;

namespace smartHookah.Services.Media
{
    public interface IMediaService
    {
        Task<Models.Db.Place.Media> AddPlacePicture(int id, HttpPostedFile file);
        Task<Models.Db.Place.Media> AddGearPicture(int id, HttpPostedFile file);

        Task<Models.Db.Place.Media> AddPlaceReviewPictureAsync(int id, HttpPostedFile file);

        Task<Models.Db.Place.Media> AddSessionReviewPictureAsync(int id, HttpPostedFile file);

        Task<Models.Db.Place.Media> AddTobaccoReviewPictureAsync(int id, HttpPostedFile file);
    }
}