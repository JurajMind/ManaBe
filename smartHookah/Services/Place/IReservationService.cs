using System.Collections.Generic;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Services.Place
{
    using System;
    using System.Threading.Tasks;

    using smartHookah.Models.Dto;

    public interface IReservationService
    {
        Task<ReservationManageDto> GetReservationManage(int id, DateTime date);

        Task<ReservationDto> CreateReservation(ReservationDto reservation);

        Task<ReservationUsage> GetReservationUsage(int placeId, DateTime date);

        Task<ReservationUsage> UpdateReservationUsage(int placeId, DateTime date);

        ICollection<Reservation> GetPersonReservations();

        IEnumerable<Reservation> GetReservations(DateTime from, DateTime to);

        Task<bool> UpdateReservationState(int id, ReservationState state);

        Task<Reservation> GetReservation(int id);

        Task<Reservation> AddTable(int id, int tableId);

        Task<Reservation> RemoveTable(int id, int tableId);

    }
}