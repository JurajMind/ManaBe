using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using smartHookah.Models.Dto;
using smartHookah.Services.Gear;

namespace smartHookah.Controllers.Api
{
    using System.Threading.Tasks;

    using smartHookah.ErrorHandler;
    using smartHookah.Services.Person;

    [RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private readonly IPersonService personService;
        private readonly IGearService gearService;

        public PersonController(IPersonService personService, IGearService gearService)
        {
            this.personService = personService;
            this.gearService = gearService;
        }

        #region Getters

        [HttpGet]
        [ApiAuthorizeAttribute,Route("InitData")]
        public async Task<PersonActiveDataDto> GetPersonActiveData()
        {
            var person = this.personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var standTask = await this.personService.GetUserActiveStands(personId);
            var devices = standTask.Select(DeviceSimpleDto.FromModel).ToList();
            var sessions = this.personService.GetUserActiveSessions(personId)
                .Select(SmokeSessionSimpleDto.FromModel)
                .ToList();
            var reservations = this.personService.GetUserActiveReservations(personId)
                .Select(Models.Dto.ReservationDto.FromModel).ToList();
            var orders = this.personService.GetUserHookahOrders(personId).Select(HookahOrderDto.FromModel).ToList();
            var gameProfile = GameProfileSimpleDto.FromModel(this.personService.GetUserGameProfile(personId));

            return new PersonActiveDataDto()
            {
                ActiveSmokeSessions = sessions,
                Devices = devices,
                ActiveReservations = reservations,
                ActiveHookahOrders = orders,
                GameProfile = gameProfile
            };
        }

        [ApiAuthorize, HttpGet, Route("Info")]
        public PersonInfoDto GetPersonInfo()
        {
            try
            {
                var user = personService.GetCurrentUser();
                return new PersonInfoDto()
                {
                    DisplayName = user.Person.DisplayName,
                    Email = user.Email,
                    ManagedPlaces = PlaceSimpleDto.FromModelList(user.Person.Manage).ToList(),
                    Roles = personService.GetUserRoles(user.Id)
                };
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message));
            }
        }

        [HttpGet, ApiAuthorizeAttribute, Route("MyGear")]
        public IEnumerable<PipeAccesorySimpleDto> MyGear(string type = "All")
        {
            var accessories = gearService.GetPersonAccessories(null, type);
            foreach (var acc in accessories)
            {
                yield return PipeAccesorySimpleDto.FromModel(acc);
            }
        }

        #endregion
    }
}