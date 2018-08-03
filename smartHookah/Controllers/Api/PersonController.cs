using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Controllers.Api
{
    using System.Threading.Tasks;

    using smartHookah.Models.Redis;
    using smartHookah.Services.Person;

    [RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public PersonController(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        [HttpGet]
        [Route("GetPersonActiveData")]
        public async Task<PersonActiveDataDTO> GetPersonActiveData()
        {
            var person = this.personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var standTask = await this.personService.GetUserActiveStands(personId);
            var stands = standTask.Select(HookahSimpleDto.FromModel).ToList();
            var sessions = this.personService.GetUserActiveSessions(personId)
                .Select(SmokeSessionSimpleDto.FromModel)
                .ToList();
            var reservations = this.personService.GetUserActiveReservations(personId)
                .Select(Models.Dto.ReservationDto.FromModel).ToList();
            var orders = this.personService.GetUserHookahOrders(personId).Select(HookahOrderDto.FromModel).ToList();
            var gameProfile = GameProfileSimpleDto.FromModel(this.personService.GetUserGameProfile(personId));

            return new PersonActiveDataDTO()
            {
                ActiveSmokeSessions = sessions,
                Stands = stands,
                ActiveReservations = reservations,
                ActiveHookahOrders = orders,
                GameProfile = gameProfile
            };
        }
    }
}