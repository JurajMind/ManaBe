using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace smartHookah.Filters
{
    using Newtonsoft.Json;
    using System.Net;
    using System.Threading.Tasks;

    public class IPGeographicalLocation
    {
        [JsonProperty("ip")]
        public string IP { get; set; }

        [JsonProperty("country_code")]

        public string CountryCode { get; set; }

        [JsonProperty("country_name")]

        public string CountryName { get; set; }

        [JsonProperty("region_code")]

        public string RegionCode { get; set; }

        [JsonProperty("region_name")]

        public string RegionName { get; set; }

        [JsonProperty("city")]

        public string City { get; set; }

        [JsonProperty("zip_code")]

        public string ZipCode { get; set; }

        [JsonProperty("time_zone")]

        public string TimeZone { get; set; }

        [JsonProperty("latitude")]

        public float Latitude { get; set; }

        [JsonProperty("longitude")]

        public float Longitude { get; set; }

        [JsonProperty("metro_code")]

        public int MetroCode { get; set; }

        private IPGeographicalLocation() { }

        public static async Task<IPGeographicalLocation> QueryGeographicalLocationAsync(string ipAddress)
        {
            WebClient client = new WebClient();

            string result = client.DownloadString("http://freegeoip.net/json/" + ipAddress);

            return JsonConvert.DeserializeObject<IPGeographicalLocation>(result);
        }
    }

    public class LocalizationAttribute : ActionFilterAttribute
    {
        private readonly string _DefaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];

        public LocalizationAttribute(string defaultLanguage)
        {
            _DefaultLanguage = defaultLanguage;
        }

        public string GetDefaultLocalitazion(string ip)
        {
            IPGeographicalLocation model = IPGeographicalLocation.QueryGeographicalLocationAsync(ip).Result;
            return model.CountryCode;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var lang = (string)filterContext.RouteData.Values["lang"] ?? _DefaultLanguage;
            var cookieLang = GetLanguage(filterContext); ;

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

        private string GetLanguage(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies.Get("manapipes-lang");
            DateTime now = DateTime.Now;


            if (cookie == null)
            {
                var userLanguages = filterContext.HttpContext.Request.UserLanguages;
                if (userLanguages != null && userLanguages.Count() > 0)
                {
                    return userLanguages.First();
                }
                else
                {
                    return this._DefaultLanguage;
                }
            }
            // Set the cookie value.
            return cookie.Value;
        }



        private static void StoreLanguage(ActionExecutingContext filterContext, string lang)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies.Get("manapipes-lang");
            if (cookie == null)
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