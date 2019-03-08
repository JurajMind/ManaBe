using smartHookah.Models.Db;

namespace smartHookah.Services.Messages
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNet.SignalR;

    using smartHookah.Hubs;
    using smartHookah.Models;
    using smartHookah.Models.Dto;
    using smartHookah.Services.Redis;

    public class NotificationService : INotificationService
    {
        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        private readonly SmartHookahContext db;

        private readonly IRedisService redisService;

        public NotificationService(SmartHookahContext db, IRedisService redisService)
        {
            this.db = db;
            this.redisService = redisService;
        }

        public void OnlineDevice(string code)
        {
            var device = this.db.Hookahs.Where(a => a.Code == code).Include(d => d.Owners).FirstOrDefault();
            var sessionCode = this.redisService.GetSessionId(code);

            foreach (var emails in device.Owners.SelectMany(s => s.User.Select(u => u.Email)))
            {
                this.ClientContext.Clients.Group(emails).deviceOnline(sessionCode,  device.Name);
            }
        }

        public void ReservationChanged(Reservation reservation)
        {
            var dBreservation = this.db.Reservations.Include(r => r.Customers).Include(r => r.Person.User)
                .FirstOrDefault(r => r.Id == reservation.Id);

            this.ReservationChanged(reservation.PlaceId);
            foreach (var customerEmails in dBreservation.Customers.SelectMany(s => s.User.Select(u => u.Email)))
            {
                this.ClientContext.Clients.Group(customerEmails).ReservationChanged(ReservationDto.FromModel(reservation));
            }

            foreach (var personEmail in dBreservation.Person.User.Select(s => s.Email))
            {
                this.ClientContext.Clients.Group(personEmail).ReservationChanged(ReservationDto.FromModel(reservation));
            }
        }

        public void ReservationChanged(int PlaceId)
        {
            ClientContext.Clients.Group($"place_{PlaceId.ToString()}").reloadReservations();
        }
    }
}