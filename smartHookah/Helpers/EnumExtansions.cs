namespace smartHookah.Helpers
{
    using System;

    using smartHookah.Models;
    using smartHookah.Models.Dto;

    public static class EnumExtansions
    {
        public static string GetName(this AccesoryType value)
        {
            return Enum.GetName(typeof(AccesoryType), value);
        }
    }
}