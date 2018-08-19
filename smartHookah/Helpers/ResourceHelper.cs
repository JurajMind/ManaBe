using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Helpers
{
    using System.IO;
    using System.Reflection;

    public static class ResourceHelper
    {
        public static string ReadResources(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = resourcePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}