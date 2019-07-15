using System.Linq;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Services.Person;

namespace smartHookah.Services.Messages
{
    public class ReservationEmailService : IReservationEmailService
    {
        private readonly IEmailService emailService;

        private readonly IPersonService personService;

        public ReservationEmailService(IEmailService emailService, IPersonService personService)
        {
            this.emailService = emailService;
            this.personService = personService;
        }

        public void CreatedReservation(Reservation reservation)
        {
           
            var template = reservation.Status == ReservationState.ConfirmationRequired
                ? "reservationWaitForConfirm"
                : "reservationConfirm";

            if(personService.IsPlaceManager(reservation.PlaceId) && reservation.PersonId != 1)
                return;

            foreach (var personEmail in reservation.Person.User.Select(s => s.Email))
            {
                this.emailService.SendTemplateAsync(personEmail, template, template, reservation);
            }

        }

        public void StateChanged(Reservation reservation)
        {
            var template = "";
            switch (reservation.Status)
            {
                case ReservationState.Confirmed:
                    template = "reservationConfirmManual";
                    break;
                case ReservationState.Canceled:
                    template = "reservationDenied";
                    break;
                case ReservationState.UnConfirmed:
                    template = "reservationDenied";
                    break;
            }
            if (personService.IsPlaceManager(reservation.PlaceId) && reservation.PersonId != 1)
                return;

            foreach (var personEmail in reservation.Person.User.Select(s => s.Email))
            {
                this.emailService.SendTemplateAsync(personEmail, template, template, reservation);
            }

        }
    }
}