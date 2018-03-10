using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    [Authorize(Roles ="Admin")]
    public class TestController : Controller
    {
        // GET: Test
        public async Task Send()
        {
            var emailService = new EmailService();
            emailService.SendTemplateAsync("jurko@bdi.sk", "Test", "test.cshtml", null);
           
        }
    }
}