using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using log4net;
using smartHookah.Models.Db;

namespace smartHookah.Services.Messages
{

    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private static FcmClientSettings settings;

        private readonly ILog logger = LogManager.GetLogger(typeof(FirebaseNotificationService));
        private readonly SmartHookahContext db;


        public FirebaseNotificationService( SmartHookahContext db)
        {
            this.db = db;

            if (settings == null)
            {
                var mappedPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Configs/manapipes-firebase-adminsdk-lkavm-60a4682f46.json");
                settings = FileBasedFcmClientSettings.CreateFromFile(mappedPath);
            }
        }

        public async Task<bool> NotifyAsync(int personId, string title, string body,Dictionary<string,string> data)
        {
            try
            {
                if (data == null)
                {
                    data = new Dictionary<string, string>();
                }
                data.Add("click_action", "FLUTTER_NOTIFICATION_CLICK");
                using (var client = new FcmClient(settings))
                {
                    var notification = new Notification
                    {
                        Title = title,
                        Body = body
                    };

                    var tokens = await this.db.NotificationTokens.Where(a => a.PersonId == personId).Select(s => s.Token)
                        .ToListAsync();

                    var messages = tokens.Select(t => new FcmMessage
                    {
                        ValidateOnly = false,
                        Message = new Message
                        {
                            Token = t,
                            Notification = notification,
                            Data = data
                        },

                    }).ToList();
                 

              
                    CancellationTokenSource cts = new CancellationTokenSource();
                    Task[] tasks = messages.Select(m => client.SendAsync(m, cts.Token)).ToArray();

                    await Task.WhenAll(tasks);


                }
            }
            catch (Exception ex)
            {
                logger.Error($"Exception thrown in Notify Service: {ex}");
            }

            return false;
        }
    }
}