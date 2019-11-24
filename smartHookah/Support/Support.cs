using Microsoft.Owin;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace smartHookah.Support
{
    public static class Support
    {
        public static IOwinContext GetOwinContext(this HttpRequestMessage request)
        {
            var context = request.Properties["MS_HttpContext"] as HttpContextWrapper;
            if (context != null)
            {
                return HttpContextBaseExtensions.GetOwinContext(context.Request);
            }
            return null;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}