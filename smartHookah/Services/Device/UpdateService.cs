using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using smartHookah.Models.Db;

namespace smartHookah.Services.Device
{
    public class UpdateService : IUpdateService
    {
        private readonly SmartHookahContext db;

        public UpdateService(SmartHookahContext db)
        {
            this.db = db;
        }


        public async Task<ICollection<Update>> GetUpdates()
        {
             return await this.db.Updates.ToListAsync();
        }
    }
}