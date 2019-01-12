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
    }
}