using System;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class MediaController : Controller
    {
        private readonly SmartHookahContext db;

        public MediaController(SmartHookahContext db)
        {
            this.db = db;
        }

        // GET: Media
        public ActionResult Index()
        {
            return View(db.Media.ToList());
        }

        // GET: Media/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        // GET: Media/Create
        public ActionResult Create(string type,int id)
        {
            ViewBag.type = type;
            return View(id);
        }

        // POST: Media/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Authorize]
        public ActionResult Create(int id,string type)
        {
            var fileName = Request.Files.AllKeys.FirstOrDefault();
            var file = Request.Files[fileName];
            var media = new Media();
                if (file != null)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName);
                    var path = $"/Content/Media/{type}/{id}/{Path.GetFileNameWithoutExtension(file.FileName)}/";
                    Directory.CreateDirectory(Server.MapPath(path));
                    // path = path + file.FileName;
                Image sourceimage = System.Drawing.Image.FromStream(file.InputStream);
                    media.Created = DateTime.UtcNow;
                    media.Type = MediaType.Picture;
                    ScaleAndSave(sourceimage, 160, 90,path,Server);
                    ScaleAndSave(sourceimage, 800, 450, path, Server);
                    ScaleAndSave(sourceimage, 1600, 900, path, Server);
                    // path = path + "image";
                    file.SaveAs(Server.MapPath(path+"original.jpg"));
                    media.Path = path;
                    db.Media.Add(media);

                    if (type == "place")
                    {
                        var place = this.db.Places.Find(id);
                        place?.Medias.Add(media);

                    }
                    if (type == "accesory")
                    {
                        var accesory = this.db.PipeAccesories.Find(id);
                        accesory?.Mediae.Add(media);
                      
                }

                db.SaveChanges();
            }

            
            
            return RedirectToAction("Index");
        }

         public static void ScaleAndSave(Image image, int maxHeight, int quality,string path, HttpServerUtilityBase server)
        {
            var scaled = ScaleImage(image, maxHeight);
            SaveImg(scaled, path, quality,server);

        }

         public static System.Drawing.Image ScaleImage(System.Drawing.Image image, int maxHeight )
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

         public static bool SaveImg(Image image,string path,int quality,HttpServerUtilityBase server)
        {
            ImageCodecInfo jpgInfo = ImageCodecInfo.GetImageEncoders()
                .Where(codecInfo => codecInfo.MimeType == "image/jpeg").First();
            using (EncoderParameters encParams = new EncoderParameters(1))
            {
                encParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                //quality should be in the range [0..100]
                using (var fileStream = System.IO.File.Create(server.MapPath(path+image.Height+".jpg")))
                {
                    image.Save(fileStream, jpgInfo, encParams);
                }

               
            }
            return true;

        }


        // GET: Media/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        // POST: Media/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [System.Web.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Path,Type")] Media media)
        {
            if (ModelState.IsValid)
            {
                db.Entry(media).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(media);
        }

        // GET: Media/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Media media = db.Media.Find(id);
            if (media == null)
            {
                return HttpNotFound();
            }
            return View(media);
        }

        // POST: Media/Delete/5
        [System.Web.Mvc.HttpPost, System.Web.Mvc.ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Media media = db.Media.Find(id);
            db.Media.Remove(media);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {          
            base.Dispose(disposing);
        }
    }
}
