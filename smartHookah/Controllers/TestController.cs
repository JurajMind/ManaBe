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
        private readonly IEmailService EmailService;

        public TestController(IEmailService emailService)
        {
            this.EmailService = emailService;
        }
        // GET: Test
        public async Task Send()
        {
            this.EmailService.SendTemplateAsync("jurko@bdi.sk", "Test", "test.cshtml", null);
           
        }
    }
}