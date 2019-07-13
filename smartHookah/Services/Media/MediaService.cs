using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Microsoft.Ajax.Utilities;
using smartHookah.Models.Db;

namespace smartHookah.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly SmartHookahContext db;
        private int[] sizes = {180, 300, 800, 1600};
        public MediaService(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<Models.Db.Media> AddPlacePicture(int id, HttpPostedFile file)
        {
            var place = await this.db.Places.FindAsync(id);
            var path = $"/Content/Place/";
            var key = place.Id.ToString();
            var media = SaveMedia(file, path, key);

            place.Medias.Add(media);

            db.SaveChanges();
            return media;
        }

        public async Task<Models.Db.Media> AddGearPicture(int id, HttpPostedFile file)
        {
            var gear = await this.db.PipeAccesories.FindAsync(id);
            var path = $"/Content/Gear/{gear.BrandName}/";
            var key = gear.Id.ToString();
            var media = SaveMedia(file, path, key);
            gear.Mediae.Add(media);

            db.SaveChanges();
            return media;
        }

        private Models.Db.Media SaveMedia(HttpPostedFile file, string path, string key)
        {
            
            if (file != null)
            {
                var media = new Models.Db.Media();
                var lastId = Guid.NewGuid().ToString().Substring(0, 15);
                var extension = Path.GetExtension(file.FileName);
                var scalePath = path + key + "/" + lastId +"/";
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

                file.SaveAs(System.Web.Hosting.HostingEnvironment.MapPath(scalePath + "original"+extension));

                return media;
            }

            return null;
        }

        private  void ScaleAndSave(Image image, int maxHeight, int quality, string path,string extension)
        {
            var scaled = ScaleImage(image, maxHeight);
            SaveImg(scaled, path, quality,extension);
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

        public bool SaveImg(Image image, string path, int quality,string extension)
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
            return MimeMapping.GetMimeMapping("file"+extension);
        }

    }
}