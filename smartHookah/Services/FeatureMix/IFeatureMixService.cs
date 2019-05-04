using System.Collections.Generic;
using smartHookah.Models.Db;

namespace smartHookah.Services.FeatureMix
{
    public interface IFeatureMixService
    {
        FeatureMixCreator GetFeatureMixCreator(int id);

        IList<FeatureMixCreator> GetFeatureMixCreators();

        IList<FeatureMixCreator> GetFollowedMixCreators();

        void AddFollow(int creatorId);

        void RemoveFollow(int creatorId);

        void CreateFeatureMixCreatorFromOld();
    }
}
