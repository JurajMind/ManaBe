﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    public class ResourcesController : Controller
    {
        public ActionResult Manifest()
        {
            var pages = new List<string>();

            var scriptsPaths = GetRelativePathsToRoot("~/Scripts/");
            var contentPaths = GetRelativePathsToRoot("~/Content/");

            var cacheResources = new List<string>();
            cacheResources.AddRange(pages);
            cacheResources.AddRange(contentPaths);
            cacheResources.AddRange(scriptsPaths);

            var manifestResult = new ManifestResult("1.0")
            {
                NetworkResources = new string[] { "" },
                CacheResources = cacheResources
            };

            return manifestResult;
        }

        public ActionResult Dummy()
        {
            return View();
        }

        private IEnumerable<string> GetRelativePathsToRoot(string virtualPath)
        {
            var physicalPath = Server.MapPath(virtualPath);
            var absolutePaths = Directory.GetFiles(physicalPath, "*.*", SearchOption.AllDirectories);

            return absolutePaths.Select(
                x => Url.Content(virtualPath + x.Replace(physicalPath, ""))
            );
        }
    }

    public class ManifestResult : FileResult
    {
        public ManifestResult(string version)
            : base("text/cache-manifest")
        {
            CacheResources = new List<string>();
            NetworkResources = new List<string>();
            FallbackResources = new Dictionary<string, string>();
            Version = version;
        }

        public string Version { get; set; }

        public IEnumerable<string> CacheResources { get; set; }

        public IEnumerable<string> NetworkResources { get; set; }

        public Dictionary<string, string> FallbackResources { get; set; }

        protected override void WriteFile(HttpResponseBase response)
        {
            WriteManifestHeader(response);
            WriteCacheResources(response);
            WriteNetwork(response);
            WriteFallback(response);
        }

        private void WriteManifestHeader(HttpResponseBase response)
        {
            response.Output.WriteLine("CACHE MANIFEST");
            response.Output.WriteLine("#V" + Version ?? string.Empty);
        }

        private void WriteCacheResources(HttpResponseBase response)
        {
            response.Output.WriteLine("CACHE:");
            foreach (var cacheResource in CacheResources)
                response.Output.WriteLine(cacheResource);
        }


        private void WriteNetwork(HttpResponseBase response)
        {
            response.Output.WriteLine();
            response.Output.WriteLine("NETWORK:");
            foreach (var networkResource in NetworkResources)
                response.Output.WriteLine(networkResource);
        }

        private void WriteFallback(HttpResponseBase response)
        {
            response.Output.WriteLine();
            response.Output.WriteLine("FALLBACK:");
            foreach (var fallbackResource in FallbackResources)
                response.Output.WriteLine(fallbackResource.Key + " " + fallbackResource.Value);
        }
    }
}
