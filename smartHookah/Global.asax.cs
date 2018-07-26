namespace smartHookah
{
    using System.Reflection;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;

    using Autofac;
    using Autofac.Integration.Mvc;
    using Autofac.Integration.WebApi;

    using smartHookah.Controllers;
    using smartHookah.Models;

    using RegistrationExtensions = Autofac.Integration.SignalR.RegistrationExtensions;

    public class MvcApplication : HttpApplication
    {
        protected void Application_End()
        {
#if DEBUG
#else
#endif
        }

        protected void Application_Start()
        {
            SetupInjection();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void SetupInjection()
        {
            var builder = new ContainerBuilder();
            var config = GlobalConfiguration.Configuration;
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterModule<ControllerModule>();
            builder.RegisterModule<SignalRModule>();
            builder.RegisterModule<DataModule>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}