using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;

namespace smartHookah.Controllers.Api
{
    using System.Threading.Tasks;

    using smartHookah.Models.Redis;
    using smartHookah.Services.Person;

    [RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private readonly IPersonService _personService;
        private readonly IGearService _gearService;

        public PersonController(IPersonService personService, IGearService gearService)
        {
            this._personService = personService;
            _gearService = gearService;
        }

        #region Getters

        [HttpGet]
        [Route("GetPersonActiveData")]
        public async Task<PersonActiveDataDTO> GetPersonActiveData()
        {
            var person = this._personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var standTask = await this._personService.GetUserActiveStands(personId);
            var stands = standTask.Select(HookahSimpleDto.FromModel).ToList();
            var sessions = this._personService.GetUserActiveSessions(personId)
                .Select(SmokeSessionSimpleDto.FromModel)
                .ToList();
            var reservations = this._personService.GetUserActiveReservations(personId)
                .Select(Models.Dto.ReservationDto.FromModel).ToList();
            var orders = this._personService.GetUserHookahOrders(personId).Select(HookahOrderDto.FromModel).ToList();
            var gameProfile = GameProfileSimpleDto.FromModel(this._personService.GetUserGameProfile(personId));

            return new PersonActiveDataDTO()
            {
                ActiveSmokeSessions = sessions,
                Stands = stands,
                ActiveReservations = reservations,
                ActiveHookahOrders = orders,
                GameProfile = gameProfile
            };
        }

        [HttpGet, Authorize, Route("MyGear")]
        public IEnumerable<PipeAccesorySimpleDto> MyGear()
        {
            var person = this._personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }
            var accessories = _gearService.GetPersonAccessories(person.Id);
            foreach (var acc in accessories)
            {
                yield return PipeAccesorySimpleDto.FromModel(acc);
            }
        }

        #endregion
    }
}