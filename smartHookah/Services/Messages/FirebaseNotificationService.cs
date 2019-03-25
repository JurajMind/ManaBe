using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using smartHookah.Services.Gear;

namespace smartHookah.Services.Messages
{
    public class FirebaseNotificationService : IFirebaseNotificationService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(FirebaseNotificationService));
        private string FireBase_URL = "https://fcm.googleapis.com/fcm/send";
        private string key_server;

        FirebaseNotificationService()
        {

        }

        public async Task<bool> NotifyAsync(string to, string title, string body)
        {
            try
            {
                // Get the server key from FCM console
                var serverKey = string.Format("key={0}", "Your server key - use app config");

                // Get the sender id from FCM console
                var senderId = string.Format("id={0}", "Your sender id - use app config");

                var data = new
                {
                    to, // Recipient device token
                    notification = new { title, body }
                };

                // Using Newtonsoft.Json
                var jsonBody = JsonConvert.SerializeObject(data);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var result = await httpClient.SendAsync(httpRequest);

                        if (result.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            // Use result.StatusCode to handle failure
                            // Your custom error handler here
                            logger.Error($"Error sending notification. Status Code: {result.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Exception thrown in Notify Service: {ex}");
            }

            return false;
        }
    }

    public class PushMessage
    {
        private string _to;
        private PushMessageData _notification;

        private dynamic _data;
        private dynamic _click_action;
        public dynamic data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string to
        {
            get { return _to; }
            set { _to = value; }
        }
        public PushMessageData notification
        {
            get { return _notification; }
            set { _notification = value; }
        }

        public dynamic click_action
        {
            get
            {
                return _click_action;
            }

            set
            {
                _click_action = value;
            }
        }
    }

    public class PushMessageData
    {
        private string _title;
        private string _text;
        private string _sound = "default";
        //private dynamic _content_available;
        private string _click_action;
        public string sound
        {
            get { return _sound; }
            set { _sound = value; }
        }

        public string title
        {
            get { return _title; }
            set { _title = value; }
        }
        public string text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string click_action
        {
            get
            {
                return _click_action;
            }

            set
            {
                _click_action = value;
            }
        }
    }
}