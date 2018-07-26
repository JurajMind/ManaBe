namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;

    using smartHookah.Models;

    public interface IPersonService
    {
        Person GetCurentPerson();

        Person GetCurentPerson(int? personId, bool manage = false);

        IQueryable<Person> GetCurentPersonIQuerable();

        IEnumerable<Hookah> GetUserStands();

        ICollection<Hookah> GetUserActiveStands(int? personId);

        ICollection<SmokeSession> GetUserActiveSessions(int? personId);

        ICollection<Reservation> GetUserActiveReservations(int? personId);

        ICollection<HookahOrder> GetUserHookahOrders(int? personId);

        GameProfile GetUserGameProfile(int? personId);
    }
}