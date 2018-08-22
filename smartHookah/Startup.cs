using Microsoft.Owin;

using smartHookah;

[assembly: OwinStartup(typeof(Startup))]

namespace smartHookah
{
    using System;
    using System.Configuration;

    using Hangfire;
    using Hangfire.Common;
    using Hangfire.Console;

    using Microsoft.Owin.Security.OAuth;

    using Owin;

    using smartHookah.Filters;
    using smartHookah.Jobs;

    public partial class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("SmartHookah").UseConsole();

            this.ConfigureAuth(app);

            app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireFilter() } });
            app.UseHangfireServer();

            RecurringJob.RemoveIfExists("AutoEnd");

            var manager = new RecurringJobManager();
            var AutoEnd = new SessionAutoEnd();

            var isDebug = true;

            // Debug.Assert(isDebug = true);
            var onDev = ConfigurationManager.AppSettings["idDevel"];
            if (onDev == null)
                manager.AddOrUpdate(
                    "AutoEnd",
                    Job.FromExpression(() => AutoEnd.EndSmokeSessions(null, isDebug)),
                    Cron.MinuteInterval(15));
            else manager.RemoveIfExists("AutoEnd");

            app.MapSignalR();
            this.ConfigureOAuth(app);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            var OAuthServerOptions = new OAuthAuthorizationServerOptions
                                         {
                                             AllowInsecureHttp = true,
                                             TokenEndpointPath =
                                                 new PathString("/token"),
                                             AccessTokenExpireTimeSpan =
                                                 TimeSpan.FromDays(1),
                                             Provider =
                                                 new SimpleAuthorizationServerProvider(),
                                             RefreshTokenProvider =
                                                 new SimpleRefreshTokenProvider(),
                                             AuthorizeEndpointPath =
                                                 new PathString(
                                                     "/api/Account/ExternalLogin")
                                         };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
        }
    }
}