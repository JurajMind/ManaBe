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
    [RoutePrefix("api/Gear")]
    public class GearController : ApiController
    {
        private readonly IGearService _gearService;

        public GearController(IGearService gearService)
        {
            _gearService = gearService;
        }

        [HttpPost, Authorize, Route("{id}/Vote")]
        public HttpResponseMessage Vote(int id, [FromBody] int value)
        {
            value = value < 0 ? (int) VoteValue.Dislike : value > 0 ? (int) VoteValue.Like : (int) VoteValue.Unlike;
            try
            {
                _gearService.Vote(id, (VoteValue) value);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
