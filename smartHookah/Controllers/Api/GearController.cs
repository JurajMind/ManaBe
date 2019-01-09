using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Services.Gear;

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

        [HttpPost, Authorize, Route("{id}/Vote")]
        public HttpResponseMessage Vote(int id, [FromBody] int value)
        {
            value = value < 0 ? (int) VoteValue.Dislike : value > 0 ? (int) VoteValue.Like : (int) VoteValue.Unlike;
            try
            {
                this.gearService.Vote(id, (VoteValue) value);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet,Authorize,Route("{type}/Search/{search}")]
        public List<GearService.SearchPipeAccesory> Search(string search, string type,int page = 0, int pageSize = 50)
        {
            if (Enum.TryParse<AccesoryType>(type.FirstLetterToUpper(), out var result))
            {
                return this.gearService.SearchAccesories(search, result,page,pageSize);
            }
            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Type:{type} was not recognize"));

        }

        [HttpGet, Authorize, Route("Brands/")]
        public Dictionary<AccesoryType,List<BrandGroupDto>> GetBrands()
        {
            return this.gearService.GetBrands();
        }

    }
}
