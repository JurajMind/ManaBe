using System.Collections.Generic;
using smartHookah.Models.Db;

namespace smartHookah.Services.FeatureMix
{
    public interface IFeatureMixService
    {
        FeatureMixCreator GetFeatureMixCreator(int id);

        IList<FeatureMixCreator> GetFeatureMixCreators(int page, int pageSize , string orderBy, string order);

        IList<FeatureMixCreator> GetFollowedMixCreators();

        void AddFollow(int creatorId);

        void RemoveFollow(int creatorId);

        void CreateFeatureMixCreatorFromOld();

        IList<PipeAccesory> GetCreatorMixes(int creatorId, int page, int pageSize, string orderBy, string order);
    }
}
