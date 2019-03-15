using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace smartHookah.Filters
{
    public class ClientIdFilter : Attribute, IActionFilter
    {
        public bool AllowMultiple => true;

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var result = continuation();
            var a = await result;
            var queryVars = a.RequestMessage.RequestUri.ParseQueryString();
            var clientId = queryVars["cliendId"];

            result.Wait(cancellationToken);
            return await result;
        }
    }
}