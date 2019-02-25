using System.Collections.Generic;
using smartHookah.Models;

namespace smartHookah.Services.Place
{
    using System;
    using System.Threading.Tasks;

    using smartHookah.Models.Dto;

    public interface IReservationService
    {
       Task<ReservationManageDto> GetReservationManage(int id, DateTime date);

        Task<bool> CreateReservation(ReservationDto reservation);

        Task<ReservationUsageDto> GetReservationUsage(int placeId, DateTime date);

        Task<ReservationUsageDto> UpdateReservationUsage(int placeId, DateTime date);

        ICollection<Reservation> GetPersonReservations();

        IEnumerable<Reservation> GetReservations(DateTime from, DateTime to);

        Task<bool> UpdateReservationState(int id, ReservationState state);
    }
}