using System.Threading.Tasks;
using smartHookah.Controllers;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface ISmokeViewMapper
    {
        Task<SmokeViewModel> Map(string sessionId);
    }
}