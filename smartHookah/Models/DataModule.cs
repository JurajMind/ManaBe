using Autofac;

namespace smartHookah.Models
{
    public class DataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SmartHookahContext>().AsSelf().InstancePerRequest();
        }
    }
}