using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using smartHookah.Controllers.Mobile;
using smartHookah.Helpers;
using smartHookah.Models;

namespace smartHookah
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "ActionApi",
            //    routeTemplate: "api/{controller}/{action}/{id}",
            //    defaults:new {id = RouteParameter.Optional}
            //);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional,action  = "DefaultAction" }
            );
        }
    }
}
