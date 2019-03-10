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

        public GearController(IGearService gearService)
        {
            this.gearService = gearService;
        }

        #region Getters

        [HttpGet, ApiAuthorize, System.Web.Http.Route("{type}/Search/{search}")]
        public List<GearService.SearchPipeAccesory> Search(string search, string type, int page = 0, int pageSize = 50, string searchType = "All")
        {
            if (Enum.TryParse<AccesoryType>(type.FirstLetterToUpper(), out var result))
            {
                if(Enum.TryParse<SearchType>(searchType.FirstLetterToUpper(), out var searchTypeType))
                return this.gearService.SearchAccesories(search, result, searchTypeType, page, pageSize);
            }

            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                $"Type:{type} was not recognize"));
        }

        [HttpGet, ApiAuthorize, System.Web.Http.Route("Brands/")]
        public Dictionary<AccesoryType, List<BrandGroupDto>> GetBrands()
        {
            return this.gearService.GetBrands();
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
    }
}
