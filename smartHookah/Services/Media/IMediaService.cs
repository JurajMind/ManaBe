using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace smartHookah.Services.Media
{
    public interface IMediaService
    {
        Task<Models.Db.Media> AddPlacePicture(int id, HttpPostedFile file);
        Task<Models.Db.Media> AddGearPicture(int id, HttpPostedFile file);
    }
}