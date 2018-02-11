using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace smartHookah.Filters
{
    public class LocalizationAttribute : ActionFilterAttribute
    {
        private readonly string _DefaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];

        public LocalizationAttribute(string defaultLanguage)
        {
            _DefaultLanguage = defaultLanguage;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var lang = (string)filterContext.RouteData.Values["lang"] ?? _DefaultLanguage;
            var cookieLang = GetLanguage(filterContext);;

            if (filterContext.RouteData.Values["lang"] == null)
            {
                lang = GetLanguage(filterContext);
                
            }
            else
            {
                // No stored language
                if (lang != cookieLang)
                {
                    StoreLanguage(filterContext, lang);
                }
              
            }

            if (lang != _DefaultLanguage)
            {
                try
                {
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
                }
                catch (Exception e)
                {
                    throw new NotSupportedException(string.Format("ERROR: Invalid language code '{0}'.", lang));
                }
            }

            if (filterContext.RouteData.Values["lang"] == null || Thread.CurrentThread.CurrentCulture != new CultureInfo(lang))
            {

                Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }

        }

        private static void UpdateLanguage(ActionExecutingContext filterContext, string lang)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies.Get("manapipes-lang");
            DateTime now = DateTime.Now;

            // Set the cookie value.
            cookie.Value = lang;

            // Don't forget to reset the Expires property!
            cookie.Expires = now.AddYears(50);
            filterContext.HttpContext.Response.SetCookie(cookie);
            
        }

        private  string GetLanguage(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies.Get("manapipes-lang");
            DateTime now = DateTime.Now;

            if (cookie == null)
                return _DefaultLanguage;

            // Set the cookie value.
            return cookie.Value;

        }

        private static void StoreLanguage(ActionExecutingContext filterContext, string lang)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies.Get("manapipes-lang");
            if(cookie ==null)
                cookie = new HttpCookie("manapipes-lang");
            DateTime now = DateTime.Now;

            // Set the cookie value.
            cookie.Value = lang;

            // Don't forget to reset the Expires property!
            cookie.Expires = now.AddYears(50);
            filterContext.HttpContext.Response.SetCookie(cookie);
        }
    }
}