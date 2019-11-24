using System.Web.Mvc;

namespace smartHookah.Controllers
{
    public class AssetsController : Controller
    {
        [Route("~/PrivacyPolicy")]
        // GET: Assets
        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult TermOfUssage()
        {
            return View();
        }
    }
}