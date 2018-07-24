using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

using smartHookah.Models;

namespace smartHookah.Controllers.Mobile
{
    using smartHookah.Services.Person;

    [Authorize]
    public class PersonController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public PersonController(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        [HttpGet]
        [ActionName("DefaultAction")]
        public string GetHome()
        {
            var persons = this.personService.GetCurentPersonIQuerable();

            var person = persons.Include(a => a.SmokeSessions.Select(b => b.MetaData))
                .Include(a => a.SmokeSessions.Select(b => b.Statistics)).FirstOrDefault();

            var session = person.SmokeSessions.Where(a => a.StatisticsId == null);

            return session.ToString();
        }

      
    }


}

