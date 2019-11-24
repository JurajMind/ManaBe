namespace smartHookah.Filters
{
    using log4net;
    using smartHookah.Controllers.Api;
    using System.Web.Mvc;

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