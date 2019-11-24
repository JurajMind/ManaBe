using smartHookahCommon.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace smartHookah.Filters
{
    using log4net;
    using smartHookah.Controllers.Api;
    using System.Web.Http.Filters;

    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MixologyController));

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception is ManaException)
            {
                var manaException = context.Exception as ManaException;
                context.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                context.Response.Headers.Add("ErrorCode", manaException.Code);

                context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest,
                    new
                    {
                        ErrorCode = manaException.Code,
                        ErrorMessage = context.Exception.Message,
                        RealStatusCode = (int)(manaException.InnerException is NotImplementedException || manaException.InnerException is ArgumentNullException ? HttpStatusCode.NoContent : HttpStatusCode.BadRequest),
                    },
                    new JsonMediaTypeFormatter());
            }

            logger.Error(context);
            base.OnException(context);
        }
    }
}