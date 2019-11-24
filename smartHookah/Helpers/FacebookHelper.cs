using System;
using System.Collections.Generic;

namespace smartHookah.Helpers
{
    public static class FacebookHelper
    {
        private static readonly List<String> EventPrivacyTypes = new List<String>() { "private", "public", "group", "community" };

        public static bool ValidateEventPrivacyType(string privacyType)
        {
            return EventPrivacyTypes.Contains(privacyType.ToLower());
        }
    }
}