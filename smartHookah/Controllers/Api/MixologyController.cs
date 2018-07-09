using smartHookah.Models;
using smartHookah.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/Mixology")]
    public class MixologyController : ApiController
    {
        private readonly SmartHookahContext _db;

        public MixologyController(SmartHookahContext db)
        {
            this._db = db;
        }
        
        #region Getters

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixCreators")]
        public async Task<MixCreatorsDTO> GetFeaturedMixCreators(int pageSize = 50, string orderBy = "name", string order = "asc")
        {
            var query = from b in _db.Brands
                        where b.TobaccoMixBrand
                        select b;
            if (pageSize > 0)
                query = query.Take(pageSize);

            switch (orderBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? from a in query orderby a.DisplayName ascending select a : from a in query orderby a.DisplayName descending select a;
                    break;
                case "count":
                    query = order.ToLower() == "asc" ? from a in query orderby a.PipeAccesories.Where(x => x is TobaccoMix).Count() ascending select a : from a in query orderby a.PipeAccesories.Where(x => x is TobaccoMix).Count() descending select a;
                    break;
                default:
                    return new MixCreatorsDTO() { Success = false, Message = "Invalid OrderBy value, select \"name\" or \"count\"." };
            }
            var res = query.ToList();

            if (res.Count() > 0)
            {
                var result = new MixCreatorsDTO() { Success = true, Message = $"{res.Count()} mix creators found." };
                foreach (var m in res)
                {
                    var creator = new MixCreator()
                    {
                        Name = m.Name,
                        DisplayName = m.DisplayName,
                        Picture = m.Picture,
                        MixCount = m.PipeAccesories.Where(a => a is TobaccoMix).Count()
                    };

                    result.MixCreatorsList.Add(creator);
                }
                return result;
            }

            return new MixCreatorsDTO() { Success = false, Message = "No mix creators found." };
        }
        #endregion
    }
}