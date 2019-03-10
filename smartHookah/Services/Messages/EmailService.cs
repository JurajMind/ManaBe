using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using log4net;
using Mailzory;
using Microsoft.AspNet.Identity;
using smartHookah.Helpers;
using Westwind.Globalization;

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

            if (!template.EndsWith(".cshtml"))
            {
                template = template + ".cshtml";
            }

            var translatedSubject =  DbRes.T(subject, "Email");

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
            email.SetFrom("app@manapipes.com", "Manapipes");
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
            email.SetFrom("app@manapipes.com", "Manapipes");

            // send email with BCC
            return email.SendAsync(message.Destination,message.Subject);


        }
    }
}