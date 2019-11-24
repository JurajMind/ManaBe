using smartHookah.Filters;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace smartHookah
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
            filters.Add(new LocalizationAttribute(ConfigurationManager.AppSettings["DefaultLanguage"]), 0);
            if (!HttpContext.Current.IsDebuggingEnabled)
            {
                filters.Add(new OptionalHttpsAttribute());
            }
            filters.Add(new ExceptionFilter());

        }
    }

    public class OptionalHttpsAttribute : RequireHttpsAttribute
    {
        private readonly bool v;

        public OptionalHttpsAttribute(bool v = false)
        {
            this.v = v;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (v)
                return;

            base.OnAuthorization(filterContext);
        }


    }
}
