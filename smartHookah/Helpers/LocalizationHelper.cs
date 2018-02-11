using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using smartHookah.Resources.Enums;
using Smoke;
using Westwind.Globalization;

namespace smartHookah.Helpers
{



    public static class LocalizationHelper
    {
        public static HtmlString Translate(string id, string set)
        {
            string text = DbRes.T(id, set);

            var span = new TagBuilder("span");
            span.Attributes.Add("data-resource-id", id);

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
