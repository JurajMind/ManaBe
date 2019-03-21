using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Models.ViewModel.SmokeSession;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface ISmokeSessionStatisticModelMapper
    {
        SmokeSessionGetStaticsticsModel Map(int sessionId);
    }
}
