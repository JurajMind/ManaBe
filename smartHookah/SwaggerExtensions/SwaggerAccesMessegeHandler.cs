using System.Net;
using System.Net.Http;

namespace smartHookah.SwaggerExtensions
{
    using System.Threading;
    using System.Threading.Tasks;

    public class SwaggerAccessMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (IsSwagger(request) && !Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                var response = request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult(response);
            }
            else
            {
                return base.SendAsync(request, cancellationToken);
            }
        }

        private bool IsSwagger(HttpRequestMessage request)
        {
            return request.RequestUri.PathAndQuery.StartsWith("/swagger");
        }
    }
}
