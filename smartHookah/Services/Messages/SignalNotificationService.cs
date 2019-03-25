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

    public class SignalNotificationService : ISignalNotificationService
    {
        private IHubContext ClientContext => GlobalHost.ConnectionManager.GetHubContext<SmokeSessionHub>();

        private readonly SmartHookahContext db;

        private readonly IRedisService redisService;
        private readonly IEmailService emailService;


        public SignalNotificationService(SmartHookahContext db, IRedisService redisService, IEmailService emailService)
        {
            this.db = db;
            this.redisService = redisService;
            this.emailService = emailService;
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

        public async void ReservationChanged(Reservation reservation)
        {
           this.ReservationChanged(reservation,null);
        }

        private  async void ReservationChanged(Reservation reservation, string emailTemplate)
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

        public void ReservationStateChanged(Reservation reservation)
        {
                this.ReservationChanged(reservation);
        }

        public void ReservationCreated(Reservation reservation)
        {
            this.ReservationChanged(reservation);
        }

        public void ReservationChanged(int PlaceId)
        {
            ClientContext.Clients.Group($"place_{PlaceId.ToString()}").reloadReservations();
        }

        public void SessionSettingsChanged(string deviceId, DeviceSetting setting)
        {
            var sessionCode = this.redisService.GetSessionId(deviceId);
            ClientContext.Clients.Group(sessionCode).settingChanged(DeviceSettingDto.FromModel(setting));
        }
    }
}