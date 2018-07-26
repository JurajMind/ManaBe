using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Services.Config
{
    public class ConfigService : IConfigService
    {
        public int DynamicSessionUpdateTime { get; set; } = 5 * 60;

        public int DynamicSessionPufCount { get; set; } = 8;
    }

    public interface IConfigService
    {
        int DynamicSessionUpdateTime { get; set; }
        int DynamicSessionPufCount { get; set; }
    }
}