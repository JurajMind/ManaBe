using smartHookah.Models.Db;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace smartHookah.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly SmartHookahContext db;
        private int[] sizes = { 180, 300, 800, 1600 };
        public MediaService(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<Models.Db.Place.Media> AddPlacePicture(int id, HttpPostedFile file)
        {
            var place = await this.db.Places.FindAsync(id);
            var path = $"/Content/Place/";
            var key = place.Id.ToString();
            var media = SaveMedia(file, path, key);

            place.Medias.Add(media);

            db.SaveChanges();
            return media;
        }

        public async Task<Models.Db.Place.Media> AddGearPicture(int id, HttpPostedFile file)
        {
            var gear = await this.db.PipeAccesories.FindAsync(id);
            var path = $"/Content/Gear/{gear.BrandName}/";
            var key = gear.Id.ToString();
            var media = SaveMedia(file, path, key);
            gear.Mediae.Add(media);

            db.SaveChanges();
            return media;
        }

        private Models.Db.Place.Media SaveMedia(HttpPostedFile file, string path, string key)
        {

            if (file != null)
            {
                var media = new Models.Db.Place.Media();
                var lastId = Guid.NewGuid().ToString().Substring(0, 15);
                var extension = Path.GetExtension(file.FileName);
                var scalePath = path + key + "/" + lastId + "/";
                media.Path = scalePath;
                media.Sizes = Json.Encode(sizes);
                media.Created = DateTime.Now;
                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath(path));
                Directory.CreateDirectory(System.Web.Hosting.HostingEnvironment.MapPath(scalePath));
                var sourceimage = Image.FromStream(file.InputStream);
                foreach (int size in sizes)
                {
                    ScaleAndSave(sourceimage, size, 80, scalePath, extension);
                }

                file.SaveAs(System.Web.Hosting.HostingEnvironment.MapPath(scalePath + "original" + extension));

                return media;
            }

            return null;
        }

        private void ScaleAndSave(Image image, int maxHeight, int quality, string path, string extension)
        {
            var scaled = ScaleImage(image, maxHeight);
            SaveImg(scaled, path, quality, extension);
        }
        private System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxHeight)
        {
            var ratio = (double)maxHeight / image.Height;
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            var newImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public bool SaveImg(Image image, string path, int quality, string extension)
        {

            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders().First(codecInfo => codecInfo.MimeType == extensionToMineType(extension));
            if (jpgInfo == null)
            {
                jpgInfo = ImageCodecInfo.GetImageEncoders().First(codecInfo => codecInfo.MimeType == "image/jpeg");
            }
            using (EncoderParameters encParams = new EncoderParameters(1))
            {
                encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                //quality should be in the range [0..100]
                using (var fileStream = System.IO.File.Create(System.Web.Hosting.HostingEnvironment.MapPath(path + image.Height + extension)))
                {
                    image.Save(fileStream, jpgInfo, encParams);
                }
            }
            return true;

        }

        string extensionToMineType(string extension)
        {
            return MimeMapping.GetMimeMapping("file" + extension);
        }

        public async Task<Models.Db.Place.Media> AddPlaceReviewPictureAsync(int id, HttpPostedFile file)
        {
            var placeReview = await this.db.PlaceReviews.FindAsync(id);
            var path = $"/Content/PlaceReview/";
            var key = placeReview.Id.ToString();
            var media = SaveMedia(file, path, key);

            placeReview.Medias.Add(media);

            db.SaveChanges();
            return media;
        }

        public async Task<Models.Db.Place.Media> AddSessionReviewPictureAsync(int id, HttpPostedFile file)
        {
            var sessionReview = await this.db.SessionReviews.FindAsync(id);
            var path = $"/Content/Sessions/";
            var key = sessionReview.Id.ToString();
            var media = SaveMedia(file, path, key);

            sessionReview.Medias.Add(media);
            if (sessionReview.TobaccoReview != null)
            {
                media.PipeAccessoryReview = sessionReview.TobaccoReview;
            }

            if (sessionReview.PlaceReview != null)
            {
                media.PlaceReview = sessionReview.PlaceReview;
            }

            db.SaveChanges();

            return media;
        }

        public async Task<Models.Db.Place.Media> AddTobaccoReviewPictureAsync(int id, HttpPostedFile file)
        {
            var tobaccoReview = await this.db.TobaccoReviews.FindAsync(id);
            var path = $"/Content/TobaccoReview/";
            var key = tobaccoReview.Id.ToString();
            var media = SaveMedia(file, path, key);

            tobaccoReview.Medias.Add(media);

            db.SaveChanges();
            return media;
        }
    }
}