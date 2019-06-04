using smartHookah.Models.Db;

namespace smartHookah.Services.Person
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using smartHookah.Models;

    public interface IPersonService
    {
        Models.Db.Person GetCurentPerson();

        Models.Db.Person GetCurentPerson(SmartHookahContext context);

        Models.Db.Person GetCurentPerson(int? personId, bool manage = false);

        ApplicationUser GetCurrentUser();

        List<string> GetUserRoles(string userId);

        IQueryable<Models.Db.Person> GetCurentPersonIQuerable();

        Task<IEnumerable<Hookah>> GetUserStands();

        Task<IEnumerable<Hookah>> GetAllStands();

        Task<ICollection<Hookah>> GetUserDevices(int? personId);

        Task<ICollection<Models.Db.SmokeSession>> GetUserActiveSessions(int? personId);

        ICollection<Reservation> GetUserActiveReservations(int? personId);

        ICollection<Reservation> GetUpcomingReservation(int? personId);

        ICollection<HookahOrder> GetUserHookahOrders(int? personId);

        GameProfile GetUserGameProfile(int? personId);
        
        bool IsPlaceManager(int placeId);
        void AddNotificationToken(string token);

        string GetCode();


        Task<Hookah> AddDevice(string deviceId);

        Task<Hookah> RemoveDevice(string deviceId);

    }
}