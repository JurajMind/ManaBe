using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace smartHookah.Helpers
{
    public class IFFTHelper
    {
        public static void PushToMakerConnect(string deviceId)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://maker.ifttt.com/trigger/hookah_online/with/key/boHVfDIj9R9Jwn09OPvCzu");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = $"{{ \"value1\" : \"{deviceId}\", \"value2\" : \"\", \"value3\" : \"\" }}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                
                throw;
            }
        }
    }
}