using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Hangfire;
using Hangfire.Common;
using ProcessDeviceToCloudMessages;
using smartHookah.Controllers;
using smartHookah.Hubs;
using smartHookah.Jobs;
using smartHookah.Models;
using Westwind.Globalization;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace smartHookah
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {

            SetupInjection();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
          
            //IotProcessor.Start();

#if DEBUG

#else
 
#endif


        }

        protected void Application_End()
        {


#if DEBUG

#else
    IotProcessor.End();
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
