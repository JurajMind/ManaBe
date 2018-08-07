using Autofac;

namespace smartHookah.Models
{
    using System.Linq;
    using System.Reflection;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;

    using Microsoft.Owin;

    using smartHookah.Services.Person;

    using smartHookahCommon;

    using Module = Autofac.Module;

    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmartHookahContext>().AsSelf().InstancePerRequest();
            builder.Register(s => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            builder.Register(s => HttpContext.Current.User).As<IPrincipal>();
            builder.RegisterType<OwinContextExtensionsWrapper>().As<IOwinContextExtensionsWrapper>();

            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>();

            foreach (var assembly in assemblies)
            {
                builder.RegisterAssemblyTypes(assembly)
                    .Except<SmartHookahContext>()
                    .Except<RedisService>()
                    .Except<OwinContext>()
                    .Except<IPrincipal>()
                    .Except<OwinContextExtensionsWrapper>()
                    .Where(t => t.Name.EndsWith("Service"))
                    .AsImplementedInterfaces();
            }
   
        }
    }
}