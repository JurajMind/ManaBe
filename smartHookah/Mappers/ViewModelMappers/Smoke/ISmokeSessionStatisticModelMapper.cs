using smartHookah.Models.ViewModel.SmokeSession;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface ISmokeSessionStatisticModelMapper
    {
        SmokeSessionGetStaticsticsModel Map(int sessionId);
    }
}
