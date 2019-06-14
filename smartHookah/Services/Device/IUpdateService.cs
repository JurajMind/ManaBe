using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models.Db;

namespace smartHookah.Services.Device
{
    public interface IUpdateService
    {
        Task<ICollection<Update>> GetUpdates();

        Task<(Update stable, Update beta)> GetUpdateInitInfo();

        Task<bool> UpdateDevice(int deviceId, int updateId, Models.Db.Person user, bool isAdmin);
    }
}