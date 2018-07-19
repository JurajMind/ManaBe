﻿using System;
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
    using Fleck;

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

            var server = new WebSocketServer("ws://0.0.0.0:8181");
            server.Start(socket =>
                {
                    socket.OnOpen = () => Console.WriteLine("Open!");
                    socket.OnClose = () => Console.WriteLine("Close!");
                    socket.OnMessage = message => socket.Send(message);
                });


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
