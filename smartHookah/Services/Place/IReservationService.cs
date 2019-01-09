namespace smartHookah.Services.Place
{
    using System;
    using System.Threading.Tasks;

    using smartHookah.Models.Dto;

    public interface IReservationService
    {
       Task<ReservationManageDto> GetReservationManage(int id, DateTime date);

        Task<bool> CreateReservation(ReservationDto reservation);

    }
}