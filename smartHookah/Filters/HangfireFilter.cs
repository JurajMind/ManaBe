using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire.Dashboard;
using Microsoft.Owin;

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