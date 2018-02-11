using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace smartHookah.Controllers
{
    public class AppCacheController : Controller
    {
        public ActionResult AppManifest(string id)
        {
            // Build the model
            var model = new AppCacheModel();

            model.AssemblyVersion = GetType().Assembly.GetName().Version.ToString();
            model.CacheCollection = new List<string>();
            //model.CacheCollection.Add(WriteBundle("~/bundles/jquery"));
            //model.CacheCollection.Add(WriteBundle("~/bundles/site"));
            model.CacheCollection.Add(GetPhysicalFilesToCache("~/Content/images", "*.jpg", string.Empty));
            model.CacheCollection.Add(GetPhysicalFilesToCache("~/Scripts", "*.js", string.Empty));
            model.CacheCollection.Add(GetPhysicalFilesToCache("~/Content", "*.css", string.Empty));

            return View(model);
        }

        public ActionResult Dummy(string id)
        {
            return View();
        }

        private string WriteBundle(string virtualPath)
        {
            var bundleString = new StringBuilder();
            bundleString.AppendLine(Scripts.Url(virtualPath).ToString());
            return bundleString.ToString();
        }

        private string GetPhysicalFilesToCache(string relativeFolderToAssets, string fileTypes, string cdnBucket)
        {
            var outputString = new StringBuilder();
            var folder = new DirectoryInfo(Server.MapPath(relativeFolderToAssets));
            foreach (FileInfo file in folder.GetFiles(fileTypes))
            {
                string location = !String.IsNullOrEmpty(cdnBucket) ? cdnBucket : relativeFolderToAssets;
                string outputFileName = (location + "/" + file).Replace("~", string.Empty);
                outputString.AppendLine(outputFileName);
            }
            return outputString.ToString();
        }
    }

    public class AppCacheModel
    {
        public string AssemblyVersion { get; set; }
        public List<string> CacheCollection { get; set; }
    }
}