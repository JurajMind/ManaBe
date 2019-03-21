using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smartHookah.Controllers;
using smartHookah.Models.Db;

namespace smartHookah.Mappers.ViewModelMappers.Smoke
{
    public interface IMetadataModalViewModelMapper
    {
        SmokeMetadataModalViewModel Map(string sessionId,SmokeSessionMetaData metaData, Models.Db.Person person, out SmokeSessionMetaData outMetaData);
    }
}
