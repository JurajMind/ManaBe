using Autofac;
using Autofac.Integration.Mvc;
using smartHookah.Controllers.Api;

namespace smartHookah.Controllers
{
    using smartHookah.Hubs;

    public class ControllerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(HomeController).Assembly);

         }
    }

    public class SignalRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(SmokeSessionHub).Assembly);

        }
    }

    public class ApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(StandPictureController).Assembly);
        }
    }
}