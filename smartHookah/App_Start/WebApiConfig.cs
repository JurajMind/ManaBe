using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Xml;
using Newtonsoft.Json.Serialization;
using smartHookah.Controllers.Mobile;
using smartHookah.Filters;
using smartHookah.Helpers;
using smartHookah.Helpers.Formaters;
using smartHookah.Models;
using Formatting = Newtonsoft.Json.Formatting;

namespace smartHookah
{
    using System.Threading.Tasks;
    using System.Web.Cors;
    using System.Web.Http.Cors;



    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Filters.Add(new ApiExceptionFilter());
            config.Filters.Add(new ClientIdFilter());
           // config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new ShortIsoDateFormatConverter());
            var cors = new EnableCorsAttribute("*", "*", "*");
            
            config.EnableCors(cors);
            //var jsonFormatter = new JsonMediaTypeFormatter
            //{
            //    SerializerSettings =
            //    {
            //        Formatting = (Formatting) System.Xml.Formatting.Indented,
            //        ContractResolver = new CamelCasePropertyNamesContractResolver()
            //    }
            //};
            //config.Formatters.Clear();
            //config.Formatters.Insert(0, jsonFormatter);
            config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));
           
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

            config.Routes.MapHttpRoute(
                name: "AnimControllerRoute",
                routeTemplate: "api/Anim",
                defaults: new { controller = "Animation", action = "DeafultAction", id = RouteParameter.Optional }
            );
        }
    }
}
