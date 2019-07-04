using System;

namespace smartHookahCommon.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveDiacritics(this string s)
        {

            s = s.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
               
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(s[i]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(s[i]);
                }
            }


            return sb.ToString();
        }

        public static string OnlyAlphaNumeric(this string s)
        {
            s = RemoveDiacritics(s);
            char[] arr = s.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c)
                                                  || char.IsWhiteSpace(c)
                                                  || c == '-')));
            return new string(arr);
        }
    }
}