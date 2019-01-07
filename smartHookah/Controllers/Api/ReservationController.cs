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

    }
}
