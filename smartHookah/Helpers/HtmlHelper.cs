using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Support;
using Westwind.Globalization;

namespace smartHookah.Helpers
{
    public static partial class HtmlHelperExtensions
    {
        public static MvcHtmlString DropDownListPipe(this HtmlHelper htmlHelper, string name,
            IEnumerable<PipeAccesory> selectList, int selected, object htmlAttributes)
        {
            var select = new TagBuilder("select");

            var options = "";
            TagBuilder option;
        

            foreach (var item in selectList.EmptyIfNull().OrderBy(a => a.BrandName)?.ThenBy(a => a.AccName))
            {
                option = new TagBuilder("option");
                option.MergeAttribute("value", item.Id.ToString());
                if(item.Brand == null)
                    continue;
                option.SetInnerText($"{item.Brand.DisplayName} {item.AccName}");
                if (item.Id == selected)
                {
                    option.MergeAttribute("selected", "selected");
                }
                options += option.ToString(TagRenderMode.Normal) + "\n";
            }

            select.MergeAttribute("id", name);
            select.MergeAttribute("name", name);

            select.InnerHtml = options;
            select.MergeAttributes((new RouteValueDictionary(htmlAttributes)));
            return new MvcHtmlString(select.ToString(TagRenderMode.Normal));
        }

    
    }
}
    