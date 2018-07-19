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

using GlobalConfiguration = System.Web.Http.GlobalConfiguration;

namespace smartHookah
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fleck;

    using Microsoft.Extensions.Logging;

    using Ninja.WebSockets;

    public class MvcApplication : HttpApplication
    {
        static IWebSocketServerFactory webSocketServerFactory;
        static ILogger logger;
        static ILoggerFactory loggerFactory;
        protected void Application_Start()
        {

            SetupInjection();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            StartWebServer();


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

        static async Task StartWebServer()
        {
            try
            {
                webSocketServerFactory = new WebSocketServerFactory();
                int port = 80;
                IList<string> supportedSubProtocols = new[] { "chatV1", "chatV2", "chatV3" };
                using (WebSocketServer socketServer = new WebSocketServer(webSocketServerFactory, loggerFactory, supportedSubProtocols))
                {
                    await socketServer.Listen(port);
                    logger.LogInformation($"Listening on port {port}");
                    
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
