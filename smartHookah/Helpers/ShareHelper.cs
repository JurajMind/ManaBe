﻿namespace smartHookah
{
    public static class ShareHelper
    {
        private static string BaseUrl = "http://app.manapipes.com/Share/";
        public static string GetFbShareLink(string token)
        {
            return BaseUrl + token;
        }
    }
}