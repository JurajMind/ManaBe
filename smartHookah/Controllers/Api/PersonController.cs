using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Controllers.Api
{
    using smartHookah.Services.Person;

    [System.Web.Http.RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private readonly SmartHookahContext _db;

        private readonly IPersonService _personService;

        public PersonController(SmartHookahContext db, IPersonService personService)
        {
            this._db = db;
            this._personService = personService;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetPersonActiveData")]
        public PersonActiveDataDTO GetPersonActiveData(int? personId)
        {
            var stands = _personService.GetUserActiveStands(personId);
            var sessions = _personService.GetUserActiveSessions(personId);
            var reservations = _personService.GetUserActiveReservations(personId);
            var orders = _personService.GetUserHookahOrders(personId);
            var gaming = _personService.GetUserGameProfile(personId);
            return new PersonActiveDataDTO()
            {
                GameProfile = gaming,
                ActiveHookahOrders = orders,
                ActiveReservations = reservations,
                ActiveSmokeSessions = sessions,
                ActiveStands = stands
            };
        }
    }
}
