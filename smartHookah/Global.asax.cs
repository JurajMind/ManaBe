using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;

using smartHookah.Controllers;
using smartHookah.Models;

namespace smartHookah
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fleck;

    using Hangfire;

    using Microsoft.Extensions.Logging;

    using Ninja.WebSockets;

    using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

    public class MvcApplication : HttpApplication
    {

        protected void Application_Start()
        {

            SetupInjection();
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

          
        }



        protected void Application_End()
        {


#if DEBUG
#else
#endif

        }


        private static void SetupInjection()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()); 
            builder.RegisterModule<ControllerModule>();
            builder.RegisterModule<DataModule>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

     
    }
}
