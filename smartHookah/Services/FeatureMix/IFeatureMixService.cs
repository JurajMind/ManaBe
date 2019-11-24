using smartHookah.Models.Db;
using System.Collections.Generic;

namespace smartHookah.Services.FeatureMix
{
    public interface IFeatureMixService
    {
        FeatureMixCreator GetFeatureMixCreator(int id);

        IList<FeatureMixCreator> GetFeatureMixCreators(int page, int pageSize, string orderBy, string order);

        IList<FeatureMixCreator> GetFollowedMixCreators();

        void AddFollow(int creatorId);

        void RemoveFollow(int creatorId);

        void CreateFeatureMixCreatorFromOld();

        IList<TobaccoMix> GetCreatorMixes(int creatorId, int page, int pageSize, string orderBy, string order);
    }
}
