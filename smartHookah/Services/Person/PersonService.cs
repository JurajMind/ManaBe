namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Principal;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin;

    using smartHookah.Helpers;
    using smartHookah.Models;

    public class PersonService : IPersonService
    {
        private readonly SmartHookahContext db;

        private readonly IPrincipal user;

        private readonly IOwinContext owinContext;

        public PersonService(SmartHookahContext db, IOwinContext owinContext, IPrincipal user)
        {
            this.db = db;
            this.owinContext = owinContext;

            this.user = user;
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

        public IEnumerable<Hookah> GetUserStands()
        {
            var user =this.owinContext.GetUserManager<ApplicationUserManager>()
                .FindById(this.user.Identity.GetUserId());
            if (user == null) return new List<Hookah>();

            if (this.user.IsInRole("Admin")) return this.db.Hookahs.ToList();

            if (user.Person != null) return user.Person.Hookahs.ToList();

            return new List<Hookah>();
        }

        public ICollection<Hookah> GetUserActiveStands(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }
            return db.Hookahs
                .Where(a => a.Owners.Any(x => x.Id == personId))
                .Where(a => a.SmokeSessions.Any(x => x.Statistics == null)).ToList();
        }

        public ICollection<SmokeSession> GetUserActiveSessions(int? personId)
        {
            if (personId == null)
            {
                var user = this.GetCurentPerson();
                personId = user.Id;
            }
            return db.SmokeSessions.Where(a => a.Statistics == null).Where(a => a.Persons.Any(x => x.Id == personId)).ToList();
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