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
            // Web API configuration and services

            AutoMapper.Mapper.Initialize(cfg =>
            {
                
                cfg.CreateMap<SmokeSession, SmokeSessionDto>()
                    .ForMember(customerDto => customerDto.Id, map => map.MapFrom(
                                customer => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(customer.Id))));
                
                cfg.CreateMap<SmokeSessionDto, SmokeSession>()
                .ForMember(customer => customer.Id, map => map.MapFrom(
                    customerDto => MySqlFuncs.IntParse(customerDto.Id)));

                cfg.CreateMap<PipeAccesoryDto, PipeAccesory>()
             .ForMember(customer => customer.Id, map => map.MapFrom(
                 customerDto => MySqlFuncs.IntParse(customerDto.Id)));

                cfg.CreateMap<PipeAccesory, PipeAccesoryDto>()
                 .ForMember(customerDto => customerDto.Id, map => map.MapFrom(
                             customer => MySqlFuncs.LTRIM(MySqlFuncs.StringConvert(customer.Id))));

            });



            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}"
             
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional,action  = "DefaultAction" }
            );
        }
    }
}
