using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using smartHookah.Models;

namespace smartHookah.Helpers
{
    public static class IdentityHelpers
    {
        public static string GetUserIdUni(this IIdentity identity, SmartHookahContext db)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;

            if (claimsIdentity.Claims.FirstOrDefault(x => x.Type == "UserId") != null)
            {
                return claimsIdentity.Claims.FirstOrDefault(x => x.Type == "UserId").Value;
            }

            if(claimsIdentity.Claims.FirstOrDefault(x => x.Type == "sub") != null)
            {
                var email = claimsIdentity.Claims.FirstOrDefault(x => x.Type == "sub").Value;
                return db.Users.FirstOrDefault(x => x.Email == email)?.Id;
            }

            return null;
        }

        public static string GetUserNameUni(this IIdentity identity)
        {
            var user =
                System.Web.HttpContext.Current.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (user != null && user.DisplayName != null) return user.DisplayName;

            return identity.Name;
        }
    }

    public static class UserHelper
    {


        public static IEnumerable<Hookah> GetUserStand()
        {
            var user =
                System.Web.HttpContext.Current.GetOwinContext()
                    .GetUserManager<ApplicationUserManager>()
                    .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if(user == null)
                return new List<Hookah>();
            var result = new List<Hookah>();

            if (System.Web.HttpContext.Current.User.IsInRole("Admin"))
            {
                using (var db = new SmartHookahContext())
                {
                  return  db.Hookahs.ToList();
                }
                
            }

            if (user.Person != null)
            {
                result.AddRange(user.Person.Hookahs);
            }

           // if (user.Person.IsLounge)
           // {
           //     result.AddRange(user.Person.Hookahs);
           // }

            return result;

        }

        public static Person GetCurentPerson()
        {
            var user =
              System.Web.HttpContext.Current.GetOwinContext()
                  .GetUserManager<ApplicationUserManager>()
                  .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());

            if (user == null)
                return null;

            return user.Person;

        }

     
        public static Person GetCurentPerson(SmartHookahContext db)
        {
            var userId = UserId(db);
            var user = db.Users.Find(userId);

            
            if (user == null || !user.PersonId.HasValue)
                return null;
            return
                db.Persons.Where(a => a.Id == user.PersonId.Value).Include(a => a.OwnedPipeAccesories).FirstOrDefault();
        }

        public static IQueryable<Person> GetCurentPersonIQuerable(SmartHookahContext db)
        {
            var userId = UserId(db);
            var user = db.Users.Find(userId);
            return db.Persons.Where(a => a.Id == user.PersonId.Value);
        }

        private static string UserId(SmartHookahContext db)
        {
            var userIdentity = System.Web.HttpContext.Current.User.Identity.GetUserId();
           if(userIdentity != null)
                return userIdentity;
            var userId = HttpContext.Current.User.Identity.GetUserIdUni(db);
            return userId;
        }


        internal static Person GetCurentPerson(SmartHookahContext db, int? personId, bool manage = false)
        {
            var canAccess = System.Web.HttpContext.Current.User.IsInRole("Admin");
            if (manage)
            {
                var curentPerson = GetCurentPerson(db);
                if (curentPerson.Manage.Any(a => a.PersonId == personId))
                {
                    canAccess = true;
                }
            }

            if (personId.HasValue && canAccess)
            {
                var person = db.Persons.Where(a => a.Id == personId.Value).Include(a => a.OwnedPipeAccesories).Include(a => a.Orders).FirstOrDefault();
                return person;
            }

            return GetCurentPerson(db);
        }
    }
}