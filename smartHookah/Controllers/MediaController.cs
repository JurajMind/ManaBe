using System;
using System.Collections.Generic;
using System.Data;
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
        private SmartHookahContext db = new SmartHookahContext();

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
        public ActionResult Create()
        {
            return View();
        }

        // POST: Media/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Path,Type")] Media media, HttpPostedFileBase file,
            FormCollection collection)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName);
                    var path = $"/Content/Media/{Guid.NewGuid()}/";
                    Directory.CreateDirectory(Server.MapPath(path));
                    System.Drawing.Image sourceimage =
                        System.Drawing.Image.FromStream(file.InputStream);
                    media.Created = DateTime.UtcNow;
                    ScaleAndSave(sourceimage, 500, 50,path,Server);
                    file.SaveAs(Server.MapPath(path+"original.jpg"));
                    media.Path = path;
                    db.Media.Add(media);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(media);
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
                using (var fileStream = System.IO.File.Create(server.MapPath(path+"."+image.Height+".jpg")))
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
        [HttpPost]
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
        [HttpPost, ActionName("Delete")]
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
