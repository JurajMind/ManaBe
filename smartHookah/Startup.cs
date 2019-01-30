using Microsoft.Owin;

using smartHookah;

[assembly: OwinStartup(typeof(Startup))]

namespace smartHookah
{
    using System;
    using System.Configuration;

    using EFCache;

    using Hangfire;
    using Hangfire.Common;
    using Hangfire.Console;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Cors;
    using Microsoft.Owin.Security.OAuth;

    using Owin;

    using smartHookah.Filters;
    using smartHookah.Jobs;

    public partial class Startup
    {
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public static OAuthAuthorizationServerOptions OAuthServerOptions { get; set; }
        public void Configuration(IAppBuilder app)
        {
            // EntityFrameworkCache.Initialize(new InMemoryCache());
            GlobalConfiguration.Configuration.UseSqlServerStorage("SmartHookah").UseConsole();

            this.ConfigureAuth(app);
            app.Map("/signalr", map =>
                {
                    // Setup the CORS middleware to run before SignalR.
                    // By default this will allow all origins. You can 
                    // configure the set of origins and/or http verbs by
                    // providing a cors options with a different policy.
                    map.UseCors(CorsOptions.AllowAll);
                    var hubConfiguration = new HubConfiguration
                                               {
                                                   // You can enable JSONP by uncommenting line below.
                                                   // JSONP requests are insecure but some older browsers (and some
                                                   // versions of IE) require JSONP to work cross domain
                                                   // EnableJSONP = true;
                                               };
                    // Run the SignalR pipeline. We're not using MapSignalR
                    // since this branch already runs under the "/signalr"
                    // path.
                    map.RunSignalR(hubConfiguration);
                });

            app.UseHangfireDashboard("/jobs", new DashboardOptions { Authorization = new[] { new HangfireFilter() } });
            app.UseHangfireServer();

            RecurringJob.RemoveIfExists("AutoEnd");

            var manager = new RecurringJobManager();
            var AutoEnd = new SessionAutoEnd();

            var isDebug = false;

            // Debug.Assert(isDebug = true);
            var onDev = ConfigurationManager.AppSettings["idDevel"];
            if (onDev == null)
                manager.AddOrUpdate(
                    "AutoEnd",
                    Job.FromExpression(() => AutoEnd.EndSmokeSessions(null, isDebug)),
                    Cron.MinuteInterval(15));
            else manager.RemoveIfExists("AutoEnd");

            //app.MapSignalR();
            this.ConfigureOAuth(app);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions();
            OAuthServerOptions = new OAuthAuthorizationServerOptions
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