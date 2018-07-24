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

        private string UserId()
        {
            var userIdentity = this.user.Identity.GetUserId();
            if (userIdentity != null) return userIdentity;

            var userId = this.user.Identity.GetUserIdUni(this.db);
            return userId;
        }
    }
}