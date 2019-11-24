using Hangfire.Dashboard;
using System.Web;

namespace smartHookah.Filters
{
    public class HangfireFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            return HttpContext.Current.User != null && HttpContext.Current.User.IsInRole("Admin");
        }
    }
}