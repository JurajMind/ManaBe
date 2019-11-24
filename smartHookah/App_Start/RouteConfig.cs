using System.Web.Mvc;
using System.Web.Routing;

namespace smartHookah
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("cache.manifest", "cache.manifest", new { controller = "Resources", action = "Manifest" });

            routes.MapRoute(
               "SmokeSession",                                           // Route name
               "smoke/{id}",                            // URL with parameters
               new { controller = "SmokeSession", action = "SmokeSession" }  // Parameter defaults
           );

            routes.MapRoute(
              "Share",                                           // Route name
              "Share/{id}",                            // URL with parameters
              new { controller = "Share", action = "Index" }  // Parameter defaults
          );

            routes.MapRoute("DefaultLocalized", "{lang}/{controller}/{action}/{id}",
                constraints: new { lang = @"(\w{2})|(\w{2}-\w{2})" }, // en or en-US
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
