using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Mvc;

namespace smartHookah.Controllers
{
    public class AnimationController : ApiController
    {
        [System.Web.Http.ActionName("DefaultAction")]
        public HttpResponseMessage GetAnimation(string id)
        {
            var result = string.Join(" ", Enumerable.Repeat(id, 10));
            result += '\r';
            string yourJson = result;
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(yourJson, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
