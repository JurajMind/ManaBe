namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;

    using smartHookah.Helpers;
    using smartHookah.Models;
    using smartHookah.Services.Device;
    using smartHookah.Services.Redis;

    using smartHookahCommon;

    public class PersonService : IPersonService
    {
        private readonly SmartHookahContext db;

        private readonly IPrincipal user;

        private readonly IOwinContext owinContext;

        private readonly IDeviceService deviceService;

        private readonly IRedisService redisService;

        public PersonService(SmartHookahContext db, IOwinContext owinContext, IPrincipal user, IDeviceService deviceService, IRedisService redisService)
        {
            this.db = db;
            this.owinContext = owinContext;

            this.user = user;
            this.deviceService = deviceService;
            this.redisService = redisService;
        }

        public Person GetCurentPerson()
        {
            var user = this.owinContext.GetUserManager<ApplicationUserManager>().FindById(this.user.Identity.GetUserId());

            return user?.Person;
        }

        public Person GetCurentPerson(int? personId, bool manage = false)
        {
            var userId = this.UserId();
            var user = this.db.Users.Find(userId);

            return user?.PersonId == null
                       ? null
                       : this.db.Persons.Where(a => a.Id == user.PersonId.Value).Include(a => a.OwnedPipeAccesories)
                           .FirstOrDefault();
        }

        public IQueryable<Person> GetCurentPersonIQuerable()
        {
            var userId = this.UserId();
            var user = this.db.Users.Find(userId);
            return this.db.Persons.Where(a => a.Id == user.PersonId.Value);
        }

        public async Task<IEnumerable<Hookah>> GetUserStands()
        {
            ApplicationUser curentUser = GetCurentUser();
            if (curentUser == null) return new List<Hookah>();

           

            if (curentUser.Person != null) return await this.SetOnlineState(curentUser.Person.Hookahs);

            return new List<Hookah>();
        }



        public async Task<IEnumerable<Hookah>> GetAllStands()
        {

            if (this.user.IsInRole("Admin"))
            {
                return await this.SetOnlineState(this.db.Hookahs);
            }
            return null;

        }
        private ApplicationUser GetCurentUser()
        {
            return this.owinContext.GetUserManager<ApplicationUserManager>()
                .FindById(this.user.Identity.GetUserId());
        }

        private async Task<IEnumerable<Hookah>> SetOnlineState(IEnumerable<Hookah> hookahs)
        {
            var onlineState = hookahs as Hookah[] ?? hookahs.ToArray();
            var hookahIds = onlineState.Select(a => a.Code);
            var deviceState = await this.deviceService.GetOnlineStates(hookahIds);
            foreach (var hookah in onlineState)
            {
                bool state;
                if (deviceState.TryGetValue(hookah.Code, out state))
                {
                    hookah.OnlineState = state;
                }
            }

            return onlineState;
        }

        public async Task<ICollection<Hookah>> GetUserActiveStands(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }
            return await db.Hookahs
                .Where(a => a.Owners.Any(x => x.Id == personId))
                .Where(a => a.SmokeSessions.Any(x => x.Statistics == null)).ToListAsync();
        }

        public ICollection<SmokeSession> GetUserActiveSessions(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }

            var sessions = db.SmokeSessions.Where(a => a.Statistics == null).Where(a => a.Persons.Any(x => x.Id == personId)).ToList();

            foreach (var session in sessions)
            {
               var ds =  this.redisService.GetDynamicSmokeStatistic(session.SessionId);
               session.DynamicSmokeStatistic = ds;
            }

            return sessions;
        }

        public ICollection<Reservation> GetUserActiveReservations(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }

            return db.Reservations.Where(a => a.Person.Id == personId).Where(a => a.Status == ReservationState.Confirmed).ToList();
        }

        public ICollection<HookahOrder> GetUserHookahOrders(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }

            return db.HookahOrders.Where(a => a.Person.Id == personId).ToList();
        }

        public GameProfile GetUserGameProfile(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }

            return db.GameProfiles.FirstOrDefault(a => a.Person.Id == personId);
        }

        private string UserId()
        {
            var userIdentity = this.user.Identity.GetUserId();
            if (userIdentity != null) return userIdentity;

            var userId = this.user.Identity.GetUserIdUni(this.db);
            return userId;
        }
    }
}