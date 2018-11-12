using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Support.Config
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Azure;

    public class ConfigSettings
    {
        internal class AppInsights
        {
            internal static bool IsEnabled
            {
                get
                {
                    return CloudConfigurationManager.GetSetting("AppInsights.IsEnabled") == "true";
                }
            }

            internal static string InstrumentationKey
            {
                get
                {
                    return CloudConfigurationManager.GetSetting("AppInsights.InstrumentationKey");
                }
            }
        }
    }

    public class AppInsightsHelper
    {
        public static void Initialize()
        {
            if (ConfigSettings.AppInsights.IsEnabled)
            {
                TelemetryConfiguration.Active.InstrumentationKey = ConfigSettings.AppInsights.InstrumentationKey;
            }
            log4net.Config.XmlConfigurator.Configure();

        }
    }
}