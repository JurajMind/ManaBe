using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.Services.WebApi;
using smartHookah.ErrorHandler;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Services.Gear;
using smartHookah.Services.Redis;
using smartHookah.Services.Search;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;

namespace smartHookah.Controllers.Api
{
    using MaxMind.GeoIP2.Exceptions;
    using smartHookah.Helpers;
    using smartHookah.Models.Dto;

    [RoutePrefix("api/Gear")]
    public class GearController : ApiController
    {
        private readonly IGearService gearService;
        private readonly IRedisService redisService;
        private readonly ISearchService searchService;

        public GearController(IGearService gearService, IRedisService redisService, ISearchService searchService)
        {
            this.gearService = gearService;
            this.redisService = redisService;
            this.searchService = searchService;
        }

        #region Getters

        [HttpGet, ApiAuthorize, System.Web.Http.Route("Search/{search}")]
        public async Task<List<SearchPipeAccessory>> Search(string search, string type = null, int page = 0, int pageSize = 50, string searchType = "All")
        {
           
            if (!string.IsNullOrEmpty(type) &&Enum.TryParse<AccesoryType>(type.FirstLetterToUpper(), out var result))
            {
                if(Enum.TryParse<SearchType>(searchType.FirstLetterToUpper(), out var searchTypeType))
                    if(searchTypeType == SearchType.Brand)

                return this.gearService.SearchAccessories(search, result, searchTypeType, page, pageSize);

                var azureSearchType = await searchService.Search(search, type);
                return azureSearchType.Select(s => new SearchPipeAccessory(s)).ToList();
            }
            var azureSearch = await searchService.Search(search, type);
            return azureSearch.Select(s => new SearchPipeAccessory(s)).ToList();

         }

        [HttpGet, ApiAuthorize, System.Web.Http.Route("Brands/")]
        public Dictionary<AccesoryType, List<BrandGroupDto>> GetBrands()
        {
            return this.gearService.GetBrands();
        }

        [HttpGet, ApiAuthorize, System.Web.Http.Route("Brand/{brandName}")]
        public async Task<BrandDto> GetBrand(string brandName)
        {
            var brand = await this.gearService.GetBrand(brandName);
            return BrandDto.FromModel(brand);
        }

        [HttpGet, ApiAuthorize, System.Web.Http.Route("{id}/Detail")]
        public PipeAccessoryDetailsDto GetDetails(int id)
        {
            try
            {
                var accessory = gearService.GetPipeAccessory(id);
                var usedByPerson = gearService.UsedByPerson(accessory);
                var usedWith = gearService.UsedWith(accessory);

                var ownedByPersons = gearService.OwnedByPersons(accessory);
                var ownedByPlaces = gearService.OwnedByPlaces(accessory);

                var usedWithDto = usedWith.Select(item => new UsedWithDto()
                    {
                        Accessory = PipeAccesorySimpleDto.FromModel(item.Key),
                        UsedCount = item.Value
                    }).ToList();

                var result = new PipeAccessoryDetailsDto()
                {
                    UsedByPerson = usedByPerson,
                    OwnedByPlaces = ownedByPlaces,
                    UsedWith = usedWithDto,
                    OwnedByPersons = ownedByPersons
                };

                return result;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message));
            }
        }
        [HttpGet, System.Web.Http.Route("Brands/{prefix}")]
        public List<string> GetBrandsPrefix(string prefix)
        {
            return this.redisService.GetBrands(prefix).ToList();
        }

        [HttpGet, System.Web.Http.Route("{id}/Sessions")]
        [ApiAuthorize]
        public List<SmokeSessionSimpleDto> Sessions(int id,int pageSize = 100,int page = 0)
        {
            return this.gearService.UsedInSession(id, pageSize, 0).Select(SmokeSessionSimpleDto.FromModel)
                .ToList();
        }

        [HttpGet, System.Web.Http.Route("{id}/Info")]
        [ApiAuthorize]
        public PipeAccesorySimpleDto Info(int id)
        {
            return PipeAccesorySimpleDto.FromModel(this.gearService.GetPipeAccessory(id));
        }

        #endregion

        [HttpPost, ApiAuthorize, System.Web.Http.Route("{id}/Vote")]
        public HttpResponseMessage Vote(int id, [FromBody] int value)
        {
            value = value < 0 ? (int) VoteValue.Dislike : value > 0 ? (int) VoteValue.Like : (int) VoteValue.Unlike;
            try
            {
                this.gearService.Vote(id, (VoteValue) value);
            }
            catch (Exception e)
            {
                var err = new System.Web.Http.HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        } 

        [HttpPost, ApiAuthorize, System.Web.Http.Route("Add")]
        public async Task<PipeAccesorySimpleDto> AddGear([FromBody] PipeAccesorySimpleDto accessory)
        {
            var modelAccessory = accessory.ToModel();
            var result = await this.gearService.AddGear(modelAccessory);
            return PipeAccesorySimpleDto.FromModel(result);
        }
    }
}
