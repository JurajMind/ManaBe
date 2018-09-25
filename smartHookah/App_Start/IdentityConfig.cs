using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using log4net;
using Mailzory;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using smartHookah.Models;

namespace smartHookah
{
    public class EmailService : IEmailService, IIdentityMessageService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(EmailService));
        public void SendTemplateAsync(string mailAdress,string subject,string template,object model)
        {
            if(mailAdress == null)
            {
                return;
            }

            // template path
            var viewPath = Path.Combine("~/Views/Emails", template);
            viewPath = HostingEnvironment.MapPath(viewPath);
            // read the content of template and pass it to the Email constructor
            var layoutPath = HostingEnvironment.MapPath("~/Views/Emails/_EmailLayout.cshtml");
            var layout = File.ReadAllLines(layoutPath);

            var index = Array.FindIndex(layout,a => a.Contains("#BODY#"));

            var first = layout.Take(index);
            var seccond = layout.Skip(index + 1);
                          
            var readedTemplate = File.ReadLines(viewPath);

            var compile = first.Concat(readedTemplate).Concat(seccond);
         
            var email = new Email(string.Join("", compile));
            email.ViewBag.Model = model;
            email.SetFrom("app@manapipes.com", "App smoke-o-bot");
            HostingEnvironment.QueueBackgroundWorkItem(async ct =>
            {
                try
                {
                  await email.SendAsync(mailAdress, subject);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });

          
        }


        public Task SendAsync(IdentityMessage message)
        {
            // template path
            var viewPath = Path.Combine("~/Views/Emails", "hello.cshtml");
            viewPath = HttpContext.Current.Server.MapPath(viewPath);
            // read the content of template and pass it to the Email constructor
            var template = File.ReadAllText(viewPath);

            var email = new Email(message.Body);


         
         
            // set Attachments (Optional)
            //email.Attachments.Add(new Attachment("Attachments/attach1.pdf"));
            //email.Attachments.Add(new Attachment("Attachments/attach2.docx"));

            // set your desired display name (Optional)
            email.SetFrom("app@manapipes.com", "App smoke-o-bot");

            // send email with BCC
            return email.SendAsync(message.Destination,message.Subject);


        }
    }

    public interface IEmailService
    {
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<SmartHookahContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = 
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}