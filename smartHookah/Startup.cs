using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Web;
using System.Web.Http;
using Hangfire;
using Hangfire.Common;
using Hangfire.Console;

using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ProcessDeviceToCloudMessages;
using smartHookah.Filters;
using smartHookah.Jobs;

using GlobalConfiguration = Hangfire.GlobalConfiguration;

[assembly: OwinStartupAttribute(typeof(smartHookah.Startup))]
namespace smartHookah
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("SmartHookah").UseConsole();

            ConfigureAuth(app);



            app.UseHangfireDashboard("/jobs",new DashboardOptions()
            {
                Authorization = new[] {new HangfireFilter() }
            });
            app.UseHangfireServer();

            RecurringJob.RemoveIfExists("AutoEnd");

            var manager = new RecurringJobManager();
            var AutoEnd = new SessionAutoEnd();
            
            bool isDebug = false;
            //Debug.Assert(isDebug = true);
            manager.AddOrUpdate("AutoEnd", Job.FromExpression(() => AutoEnd.EndSmokeSessions(null, isDebug)), Cron.MinuteInterval(15));
            
            app.MapSignalR();
            ConfigureOAuth(app);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider(),
                RefreshTokenProvider = new SimpleRefreshTokenProvider(),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
        
    }

  

}
