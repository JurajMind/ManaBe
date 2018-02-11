using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace smartHookah.Helpers
{
    public static class MySqlFuncs
    {
        [DbFunction("SqlServer", "STR")]
        public static string StringConvert(long number)
        {
            return number.ToString();
        }
        [DbFunction("SqlServer", "LTRIM")]
        public static string LTRIM(string s)
        {
            return s == null ? null : s.TrimStart();
        }
        // Can only be used locally.
        public static int IntParse(string s)
        {
            int ret;
            int.TryParse(s, out ret);
            return ret;
        }
    }
}