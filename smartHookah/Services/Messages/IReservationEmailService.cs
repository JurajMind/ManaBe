using smartHookah.Models.Db;

namespace smartHookah.Services.Messages
{
    public interface IReservationEmailService
    {
        void CreatedReservation(Reservation reservation);

        void StateChanged(Reservation reservation);
    }
}