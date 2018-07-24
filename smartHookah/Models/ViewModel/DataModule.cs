using Autofac;

namespace smartHookah.Models
{
    using System.Security.Principal;
    using System.Web;

    using Microsoft.Owin;

    using smartHookah.Services.Person;

    using smartHookahCommon;

    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmartHookahContext>().AsSelf().InstancePerRequest();
            builder.Register(s => new RedisService()).AsSelf().As<IRedisService>().SingleInstance().AutoActivate();
            builder.Register(s => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            builder.Register(s => HttpContext.Current.User).As<IPrincipal>();
            builder.RegisterType<OwinContextExtensionsWrapper>().As<IOwinContextExtensionsWrapper>();


            builder.RegisterType<PersonService>().As<IPersonService>();


            
        }
    }
}