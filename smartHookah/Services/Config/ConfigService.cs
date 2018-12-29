namespace smartHookah.Services.Config
{
    using System.Configuration;

    public class ConfigService : IConfigService
    {
        public string Enviroment => ConfigurationManager.AppSettings["Enviroment"];
    }
}