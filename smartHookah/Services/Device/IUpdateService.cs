using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models.Db;

namespace smartHookah.Services.Device
{
    public interface IUpdateService
    {
        Task<ICollection<Update>> GetUpdates();
    }
}