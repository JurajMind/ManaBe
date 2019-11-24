using smartHookah.Resources.Enums;
using System;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using Westwind.Globalization;

namespace smartHookah.Helpers
{



    public static class LocalizationHelper
    {
        public static string getCurentCultureString()
        {
            var isoLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            switch (isoLang)
            {
                case "en":
                    {
                        return "en-us";
                    }
                case "cs":
                    {
                        return "cs-cz";
                    }
                case "sk":
                    {
                        return "sk-sk";
                    }
                default:
                    return "en-us";
            }
        }

        public static HtmlString Translate(string id, string set)
        {
            string text = DbRes.T(id, set);

            var span = new TagBuilder("span");
            span.Attributes.Add("data-resource-id", id);
            span.Attributes.Add("data-resource-set", set);
            return new HtmlString(text + span.ToString(TagRenderMode.Normal));

        }

        private static
            ResourceManager _resources =
                new global::System.Resources.ResourceManager("smartHookah.Resources.Enums.Enums",
                    typeof(Enums).Assembly);

        public static string EnumDescription(this Enum enumerator)
        {
            string rk = String.Format("{0}.{1}", enumerator.GetType(), enumerator);
            string localizedDescription = _resources.GetString(rk);

            if (localizedDescription == null)
            {
                // A localized string was not found so you can either just return
                // the enums value - most likely readable and a good fallback.
                return enumerator.ToString();

                // Or you can return the full resourceKey which will be helpful when
                // editing the resource files(e.g. MyClass+SomeEnum.Small) 
                // return resourceKey;
            }
            else
                return localizedDescription;
        }

    }
}
