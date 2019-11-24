using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto;
using smartHookah.Services.Place;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace smartHookah.Controllers.Api.Admin
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/Admin/Places")]
    public class PlaceAdminController : ApiController
    {
        private readonly IPlaceService placeService;

        public PlaceAdminController(IPlaceService placeService)
        {
            this.placeService = placeService;
        }

        [HttpGet, Route("Waiting")]
        public async Task<List<PlaceSimpleDto>> GetWaitingPlaces()
        {
            var places = await this.placeService.GetWaitingPlaces();
            return PlaceSimpleDto.FromModelList(places).ToList();
        }

        [HttpPost, Route("{id}/ChangeState")]
        public async Task<PlaceSimpleDto> ChangeState(int id, int newState)
        {
            var places = await this.placeService.ChangePlaceState(id, (PlaceState)newState);
            return PlaceSimpleDto.FromModel(places);
        }
    }
}
