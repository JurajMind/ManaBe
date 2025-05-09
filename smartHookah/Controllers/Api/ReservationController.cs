﻿using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto.Places.Reservations;
using smartHookah.Models.Dto.Reservations;
using smartHookah.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    using smartHookah.ErrorHandler;
    using smartHookah.Models.Dto;
    using smartHookah.Services.Place;
    using System.Threading.Tasks;

    [RoutePrefix("api/Reservations")]
    public class ReservationController : ApiController
    {
        private readonly IReservationService reservationService;

        public ReservationController(IReservationService reservationService)
        {
            this.reservationService = reservationService;
        }

        [HttpGet, Route("{id}/Manage")]
        public async Task<ReservationManageDto> GetManagereservationsInfo(int id, DateTime date)
        {
            return await this.reservationService.GetReservationManage(id, date);
        }

        [HttpGet, Route("Person")]
        public IEnumerable<Models.Dto.Places.Reservations.ReservationDto> GetPersonReservations()
        {
            var reservations = this.reservationService.GetPersonReservations();
            foreach (var item in reservations) yield return Models.Dto.Places.Reservations.ReservationDto.FromModel(item);
        }

        [HttpGet, Route("Reservations")]
        [ApiAuthorize]
        public IEnumerable<Models.Dto.Places.Reservations.ReservationDto> GetReservations(DateTime from, DateTime to)
        {
            var reservations = this.reservationService.GetReservations(from, to);
            foreach (var item in reservations) yield return Models.Dto.Places.Reservations.ReservationDto.FromModel(item);
        }

        [HttpPost, Route("Create")]
        public async Task<Models.Dto.Places.Reservations.ReservationDto> Create(Models.Dto.Places.Reservations.ReservationDto reservation)
        {
            DateTime convertedDate = DateTime.SpecifyKind(
              reservation.Time,
                DateTimeKind.Utc);
            var kind = convertedDate.Kind;
            DateTime dt = convertedDate.ToLocalTime();
            reservation.Time = dt;
            return await this.reservationService.CreateReservation(reservation);
        }

        [HttpGet, Route("{id}/Usage")]
        public async Task<ReservationUsageDto> GetReservationUsage(int id, DateTime date)
        {
            var reservationUsage = await this.reservationService.GetReservationUsage(id, date);
            var result = new ReservationUsageDto
            {
                TimeSlots = reservationUsage.TimeSlots.EmptyIfNull().Select(s => new TimeSlot(s)).ToList()
            };
            return result;
        }


        [HttpPost, Route("{id}/UpdateState")]
        public async Task<bool> UpdateReservationState(int id, [FromBody]string state)
        {
            if (Enum.TryParse(state, true, out ReservationState status) && Enum.IsDefined(typeof(ReservationState), status))
            {
                return await reservationService.UpdateReservationState(id, status);
            }

            return false;
        }

        [HttpPost, Route("{id}/AddLateTime")]
        public async Task<smartHookah.Models.Dto.Places.Reservations.ReservationDto> AddLateTime(int id, [FromBody]int time)
        {
            return Models.Dto.Places.Reservations.ReservationDto.FromModel(await reservationService.AddLateTime(id, time));
        }

        [HttpPost, Route("{id}/Cancel")]
        public async Task<bool> CancelReservation(int id)
        {
            return await reservationService.UpdateReservationState(id, ReservationState.Canceled);
        }

        [HttpGet, Route("{id}/Detail")]
        public async Task<ReservationDetailDto> GetReservationDetail(int id)
        {
            var reservation = await reservationService.GetReservation(id);
            return new ReservationDetailDto()
            {
                Reservation = Models.Dto.Places.Reservations.ReservationDto.FromModel(reservation),
                Orders = HookahOrderDto.FromModelList(reservation.Orders).ToList(),
                Place = PlaceDto.FromModel(reservation.Place),
                SmokeSessions = SmokeSessionSimpleDto.FromModelList(reservation.Orders.Select(a => a.SmokeSession).ToList()).ToList()
            };
        }

        [HttpPost, Route("{id}/AddTable")]
        public async Task<Models.Dto.Places.Reservations.ReservationDto> AddTable(int id, [FromBody] int tableId)
        {
            var reservation = await reservationService.AddTable(id, tableId);
            return Models.Dto.Places.Reservations.ReservationDto.FromModel(reservation);
        }

        [HttpDelete, Route("{id}/RemoveTable")]
        public async Task<Models.Dto.Places.Reservations.ReservationDto> RemoveTable(int id, [FromBody]int tableId)
        {
            var reservation = await reservationService.RemoveTable(id, tableId);
            return Models.Dto.Places.Reservations.ReservationDto.FromModel(reservation);
        }
    }
}
