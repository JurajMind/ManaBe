using System.Collections.Generic;
using System.Linq;

namespace smartHookah.Helpers
{
    using smartHookah.Models.Dto;
    using System;

    public static class EnumExtensions
    {
        public static string GetName(this AccesoryType value)
        {
            return Enum.GetName(typeof(AccesoryType), value);
        }

        public static string GetSearchName(this AccesoryType value)
        {
            switch (value)
            {
                case AccesoryType.Bowl:
                    return "bowl";
                case AccesoryType.Hookah:
                    return "pipe";
                case AccesoryType.Heatmanagement:
                    return "heatManagment";
                case AccesoryType.Coal:
                    return "coal";
                case AccesoryType.Tobacco:
                    return "tobacco";
                case AccesoryType.TobaccoMix:
                    return "tobaccomix";
            }
            return "";
        }

        public static IEnumerable<Enum> GetFlags(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

    }


}