using smartHookah.Controllers;
using smartHookah.Models.Db;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface IMetadataModalViewModelMapper
    {
        SmokeMetadataModalViewModel Map(string sessionId, SmokeSessionMetaData metaData, Models.Db.Person person, out SmokeSessionMetaData outMetaData);
    }
}
