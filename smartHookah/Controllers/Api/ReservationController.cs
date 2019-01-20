using System;
using System.Web.Http;

namespace smartHookah.Controllers.Api
{
    using System.Threading.Tasks;

    using smartHookah.Models.Dto;
    using smartHookah.Services.Place;

    [RoutePrefix("api/Reservations")]
    public class ReservationController : ApiController
    {
        private readonly IReservationService reservationService;
        
        public ReservationController(IReservationService reservationService)
        {
            this.reservationService = reservationService;
        }

        [HttpGet, Route("{id}/Manage")]
        public async Task<ReservationManageDto> GetManagereservationsInfo(int id,DateTime date)
        {
            return await this.reservationService.GetReservationManage(id, date);
        }

        [HttpPost, Route("Create")]
        public async Task<bool> Create(ReservationDto reservation)
        {
            return await this.reservationService.CreateReservation(reservation);
        }

        [HttpGet, Route("{id}/Usage")]
        public async Task<ReservationUsageDto> GetReservationUsage(int id, DateTime date)
        {
            return await this.reservationService.GetReservationUsage(id, date);
        }

    }
}
