using System;

namespace smartHookahTests.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveDiacritics(string s)
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


    }
}