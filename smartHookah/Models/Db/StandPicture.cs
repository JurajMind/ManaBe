using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using smartHookah.Helpers;

namespace smartHookah.Models.Db
{
    public class StandPicture
    {
       public int Id { get; set; }
       public string PictueString { get; set; }
       
       public int Width { get; set; }

        public int Height { get; set; }

        [NotMapped]
        public string HtmlString
        {
            get
            {
                var byteArray = Convert.FromBase64String(PictueString);
                var bitmap = PictureHelper.XbmToBmp(byteArray, Height, Width);


                return ToBase64String(bitmap, ImageFormat.Bmp);
            }
        }

        public static string ToBase64String(Bitmap bmp, ImageFormat imageFormat)
        {
            string base64String = string.Empty;


            MemoryStream memoryStream = new MemoryStream();
            bmp.Save(memoryStream, imageFormat);


            memoryStream.Position = 0;
            byte[] byteBuffer = memoryStream.ToArray();


            memoryStream.Close();


            base64String = Convert.ToBase64String(byteBuffer);
            byteBuffer = null;


            return base64String;
        }

      

    }
}