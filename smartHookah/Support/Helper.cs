using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace smartHookah
{
    public static class Helper
    {
        public static int UpdateVersionToInt(string versionString)
        {
            var resultString = new StringBuilder();
            var chunk = versionString.Split('.');

            for (var i = 0; i < chunk.Length; i++)
            {
                resultString.Append(int.Parse(chunk[i]).ToString("000"));
            }

            return int.Parse(resultString.ToString());
        }

        public static string UpdateVersionToString(int version)
        {
            if (version == 1)
            {
                return "1.0.0";
            }

            var minor = version%1000;
            version = version/1000;
            var major = version % 1000;
            version = version / 1000;
            var whole = version;

            return $"{whole}.{major}.{minor}";


        }
    }
}