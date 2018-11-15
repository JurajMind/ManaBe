using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Filters
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Filters;
    using System.Web.Mvc;

    using log4net;

    using smartHookah.Controllers.Api;

    public class ExceptionFilter : HandleErrorAttribute
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(MixologyController));
        public override void OnException(ExceptionContext filterContext)
        {
            logger.Error(filterContext);
            base.OnException(filterContext);
        }
    }
}