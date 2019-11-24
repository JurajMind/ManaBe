using smartHookah.Controllers;
using System.Threading.Tasks;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface ISmokeViewMapper
    {
        Task<SmokeViewModel> Map(string sessionId);
    }
}