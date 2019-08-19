using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using smartHookah.ErrorHandler;
using smartHookah.Filters;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto;
using smartHookah.Services.Media;
using smartHookah.SwaggerExtensions;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Controllers.Api
{
    [ApiAuthorize]
    [System.Web.Http.RoutePrefix("api/Media")]
    public class MediaController : ApiController
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            this._mediaService = mediaService;
        }

        [SwaggerParameter("file", "A file", Required = true, Type = "file")]
        [HttpPost]
        [Route("Place/{id}/Add")]
        public async Task<MediaDto> AddPlacePicture(int id)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            this.ValidateFile(file);
            Media media = await this._mediaService.AddPlacePicture(id, file);
            return MediaDto.FromModel(media);
        }

        [SwaggerParameter("file", "A file", Required = true, Type = "file")]
        [HttpPost]
        [Route("Gear/{id}/Add")]
        public async Task<MediaDto> AddGearPicture(int id)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            this.ValidateFile(file);
            Media media = await this._mediaService.AddGearPicture(id, file);
            return MediaDto.FromModel(media);
        }

        [SwaggerParameter("file", "A file", Required = true, Type = "file")]
        [HttpPost]
        [Route("PlaceReview/{id}/Add")]
        public async Task<MediaDto> AddPlaceReview(int id)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            this.ValidateFile(file);
            Media media = await this._mediaService.AddPlaceReviewPictureAsync(id, file);
            return MediaDto.FromModel(media);
        }

        [SwaggerParameter("file", "A file", Required = true, Type = "file")]
        [HttpPost]
        [Route("SessionReview/{id}/Add")]
        public async Task<MediaDto> AddSessionReview(int id)
        {
            var file = HttpContext.Current.Request.Files.Count > 0 ?
                HttpContext.Current.Request.Files[0] : null;
            this.ValidateFile(file);
            Media media = await this._mediaService.AddSessionReviewPictureAsync(id, file);
            return MediaDto.FromModel(media);
        }

        private void ValidateFile(HttpPostedFile postedFile)
        {
            try
            {
                if (postedFile != null && postedFile.ContentLength > 0)
                {

                    int MaxContentLength = 1024 * 1024 * 3; //Size = 3 MB

                    IList<string> AllowedFileExtensions = new List<string> {".jpg", ".gif", ".png",".bmp"};
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        throw new ManaException(ErrorCodes.MediaUploadError,
                            "Please Upload image of type .jpg,.gif,.png,.bmp.");
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {

                        throw new ManaException(ErrorCodes.MediaUploadError,
                            "Please Upload file upto 3mb");
                    }

                    return;

                }

                throw new ManaException(ErrorCodes.MediaUploadError,
                    "Please Upload a image.");
            }
            catch (ManaException me)
            {
                throw me;
            }
            catch (Exception ex)
            {
                throw new ManaException(ErrorCodes.MediaUploadError,
                    "Upload error",ex);
            }
        }
    }
}
