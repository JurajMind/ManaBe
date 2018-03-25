using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace smartHookah.Controllers.Mobile
{
    using System.Data.Entity;
    using System.Threading.Tasks;

    using smartHookah.Helpers;
    using smartHookah.Models;

    public class SmokeSessionController : ApiController
    {
        private SmartHookahContext _db;
        public SmokeSessionController(SmartHookahContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<List<string>> Active()
        {
            var persons = UserHelper.GetCurentPersonIQuerable(_db);

            var person = persons.Include(a => a.SmokeSessions.Select(b => b.MetaData))
                .Include(a => a.SmokeSessions.Select(b => b.Statistics)).FirstOrDefault();

            var sessions = person.SmokeSessions.Where(a => a.Statistics == null);

            return sessions.Select(a => a.SessionId).ToList();
        }

        [HttpGet]
        public async Task<SmokeSessionDTO> Details(string id)
        {
            var smokeSession = this._db.SmokeSessions.Include(a => a.MetaData)
                .Include(a => a.MetaData.Pipe).Include(a => a.MetaData.Tobacco).Include(a => a.Hookah).FirstOrDefault(a => a.SessionId == id);

            return new SmokeSessionDTO(smokeSession);
        }
    }

    public class SmokeSessionDTO
    {
        public SmokeSessionDTO(SmokeSession smokeSession)
        {
            this.Id = smokeSession.SessionId;
        }

        public string Id { get; set; }
    }

    public class MetadataDto
    {
        
    }
}
