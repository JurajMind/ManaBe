using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Hangfire;
using Hangfire.Common;
using Hangfire.Console;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
using smartHookah.Models;
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
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }

        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            //config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .AddTables(new MobileAppTableConfiguration().MapTableControllers().AddEntityFramework())
                .MapApiControllers()
                //.AddPushNotification()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            
            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer<templateitemsContext>(null);

            MobileAppSettingsDictionary settings = config.GetMobileAppSettingsProvider().GetMobileAppSettings();

            if (string.IsNullOrEmpty(settings.HostName))
            {
                // This middleware is intended to be used locally for debugging. By default, HostName will
                // only have a value when running in an App Service application.
                app.UseAppServiceAuthentication(new AppServiceAuthenticationOptions
                {
                    SigningKey = ConfigurationManager.AppSettings["SigningKey"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["ValidAudience"] },
                    ValidIssuers = new[] { ConfigurationManager.AppSettings["ValidIssuer"] },
                    TokenHandler = config.GetAppServiceTokenHandler()
                });
            }
            app.UseWebApi(config);

        }
    }
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (AuthRepository _repo = new AuthRepository())
            {
                IdentityUser user = await _repo.FindUser(context.UserName, context.Password);

                if (user == null)
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);

        }
    }

    public class AuthRepository : IDisposable
    {
        private SmartHookahContext _ctx;

        private UserManager<ApplicationUser> _userManager;

        public AuthRepository()
        {
            _ctx = new SmartHookahContext();
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(_ctx));
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = userModel.UserName
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);

            return result;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await _userManager.FindAsync(userName, password);

            return user;
        }

        public void Dispose()
        {
            _ctx.Dispose();
            _userManager.Dispose();

        }
    }

    public class UserModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
