﻿using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Device;
using smartHookah.Services.Device;
using smartHookah.Services.Gear;
using smartHookah.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    using smartHookah.ErrorHandler;
    using smartHookah.Services.Person;
    using System.Threading.Tasks;

    [RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private readonly IPersonService personService;
        private readonly IGearService gearService;
        private readonly IFirebaseNotificationService firebaseNotificationService;
        private readonly IUpdateService updateService;

        public PersonController(IPersonService personService, IGearService gearService, IFirebaseNotificationService firebaseNotificationService, IUpdateService updateService)
        {
            this.personService = personService;
            this.gearService = gearService;
            this.firebaseNotificationService = firebaseNotificationService;
            this.updateService = updateService;
        }

        #region Getters

        [HttpGet]
        [ApiAuthorize, Route("InitData")]
        public async Task<PersonActiveDataDto> GetPersonActiveData()
        {
            var person = this.personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var standTask = await this.personService.GetUserDevices(personId);
            var devices = standTask.Select(DeviceSimpleDto.FromModel).ToList();
            var sessions = (await this.personService.GetUserActiveSessions(personId))
                .Select(SmokeSessionSimpleDto.FromModel)
                .ToList();
            var reservations = this.personService.GetUpcomingReservation(personId)
                .Select(Models.Dto.Places.Reservations.ReservationDto.FromModel).ToList();
            var orders = this.personService.GetUserHookahOrders(personId).Select(HookahOrderDto.FromModel).ToList();
            var gameProfile = GameProfileSimpleDto.FromModel(this.personService.GetUserGameProfile(personId));
            var updateInfo = await this.updateService.GetUpdateInitInfo();
            return new PersonActiveDataDto()
            {
                ActiveSmokeSessions = sessions,
                Devices = devices,
                ActiveReservations = reservations,
                ActiveHookahOrders = orders,
                GameProfile = gameProfile,
                UpdateInfo = new DeviceUpdateInfoDto
                {
                    StableVersion = UpdateDto.FromModel(updateInfo.stable),
                    BetaVersion = UpdateDto.FromModel(updateInfo.beta),
                }
            };
        }

        [HttpGet]
        [ApiAuthorize, Route("Sessions")]
        public async Task<ICollection<SmokeSessionSimpleDto>> GetPersonSessions()
        {
            var person = this.personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var sessions = (await this.personService.GetUserActiveSessions(personId))
                .Select(SmokeSessionSimpleDto.FromModel)
                .ToList();

            return sessions;
        }

        [HttpGet]
        [ApiAuthorize, Route("Devices")]
        public async Task<List<DeviceSimpleDto>> GetPersonDevices()
        {
            var person = this.personService.GetCurentPerson();

            if (person == null)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Person not logged"));
            }

            var personId = person.Id;
            var standTask = await this.personService.GetUserDevices(personId);
            var devices = standTask.Select(DeviceSimpleDto.FromModel).ToList();
            return devices;
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
                    Roles = personService.GetUserRoles(user.Id),
                    PersonId = personService.GetCurentPerson().Id

                };
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message));
            }
        }

        [ApiAuthorize, HttpPut, Route("NotificationToken")]
        public void AddNotificationToken(string token)
        {
            try
            {
                this.personService.AddNotificationToken(token);
            }
            catch (Exception e)
            {

            }
        }

        [ApiAuthorize, HttpPost, Route("TestNotification")]
        public async Task TestNotification()
        {
            await this.firebaseNotificationService.NotifyAsync(this.personService.GetCurentPerson().Id, "Test notifikace",
                 $"Test notification", new Dictionary<string, string>()
                 {
                    { "Date", DateTime.UtcNow.ToString() }
                 });
        }



        [HttpGet, ApiAuthorize, Route("MyGear")]
        public IEnumerable<PipeAccesorySimpleDto> MyGear(string type = "All")
        {
            var accessories = gearService.GetPersonAccessories(null, type);
            foreach (var acc in accessories)
            {
                yield return PipeAccesorySimpleDto.FromModel(acc);
            }
        }

        [HttpGet, ApiAuthorize, Route("MyGear/Used/{sessionCount}")]
        public ICollection<PipeAccesorySimpleDto> GetRecentAccessories(int sessionCount = 10)
        {
            try
            {
                return PipeAccesorySimpleDto.FromModelList(gearService.GetRecentAccessories(sessionCount)).ToList();
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message));
            }
        }

        #endregion

        #region Setters

        [HttpPost, ApiAuthorize, Route("MyGear/{id}/Add")]
        public async Task<PipeAccesorySimpleDto> AddMyGear(int id, int count = 1)
        {
            return PipeAccesorySimpleDto.FromModel(await gearService.AddMyGear(id, count, null));
        }

        [HttpPost, ApiAuthorize, Route("MyGear/{id}/Delete")]
        public async Task<bool> DeleteMyGear(int id, int? count = 0)
        {
            return await gearService.DeleteMyGear(id, count, null);
        }

        [HttpPost, ApiAuthorize, Route("AssignSession/{id}")]
        public async Task<SmokeSessionSimpleDto> AssignSession(int id)
        {
            return SmokeSessionSimpleDto.FromModel(await personService.AssignSession(id));
        }

        [HttpPost, ApiAuthorize, Route("UnAssignSession/{id}")]
        public async Task<bool> UnAssignSession(int id)
        {
            return await personService.UnAssignSession(id);
        }

        #endregion
    }
}