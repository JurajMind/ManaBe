using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using smartHookah.Helpers;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Services.Person;

namespace smartHookah.Controllers
{
    public class HomeController : Controller
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public HomeController(SmartHookahContext db, IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }

        [HttpGet]
        public ActionResult GoToSession()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult GoToSession(string id)
        {
            var sessionId = id.ToUpper();
            var session = this.db.SmokeSessions.FirstOrDefault(a => a.SessionId == sessionId);
            return session == null ? this.RedirectToAction("GoToSession") : this.RedirectToAction("SmokeSession", "SmokeSession", new { id });
        }

        public ActionResult Index()
        {
            if (this.personService.GetCurentPerson() != null)
            {
                return this.RedirectToAction("Index", "Person");
            }

            return this.View();
        }

        [Authorize]
        public async Task<ActionResult> Old()
        {
            var allStands = await this.personService.GetAllStands();

            return this.View(allStands);
        }

        public ActionResult VueTest()
        {
            return this.View();
        }
    }
}