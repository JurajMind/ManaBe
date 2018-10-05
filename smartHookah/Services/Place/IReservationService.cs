namespace smartHookah.Services.Place
{
    using System;

    using smartHookah.Controllers;

    public interface IReservationService
    {
        ReservationInfo GetReservation(int id, DateTime date, bool includeReservation = false);
    }
}