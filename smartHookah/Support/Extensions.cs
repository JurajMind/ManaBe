using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using smartHookah.Models;
using ServiceStack;
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace smartHookah.Support
{
    public static class Extensions
    {
        public static MvcHtmlString FaActionLink(this AjaxHelper ajaxHelper, string text,string fontAwesome, string actionName, string controllerName, object routeValues, AjaxOptions ajaxOptions, object htmlAttributes)
        {
            var repID = Guid.NewGuid().ToString();
            var lnk = ajaxHelper.ActionLink(repID, actionName, controllerName, routeValues, ajaxOptions, htmlAttributes);
            var tag = new TagBuilder("i");
            tag.MergeAttribute("class",fontAwesome);
            
            return MvcHtmlString.Create(lnk.ToString().Replace(repID, text + "&nbsp;"+tag.ToString(TagRenderMode.Normal)));
        }

        public static SmokeSessionMetaData Merge(List<SmokeSessionMetaData> metaDatas,SmartHookahContext db)
        {
            var result = new SmokeSessionMetaData();

            metaDatas.Reverse();

            foreach (var meta in metaDatas)
            {
                if(meta == null)
                    continue;
                
                if (meta.Id != 0)
                    result.Id = 0;

               result.Copy(meta);


            }

            if (result.PipeId != null && result.Pipe == null)
                result.Pipe = db.Pipes.Find(result.PipeId);


            if (result.BowlId != null && result.Bowl == null)
                result.Bowl = db.Bowls.Find(result.BowlId);

            if (result.TobaccoId != null && result.Tobacco == null)
                result.Tobacco = db.Tobaccos.Find(result.TobaccoId);

            return result;
        }
        public static CultureInfo ResolveCulture()
        {
            string[] languages = HttpContext.Current.Request.UserLanguages;

            if (languages == null || languages.Length == 0)
                return null;

            try
            {
                string language = languages[0].ToLowerInvariant().Trim();
                return CultureInfo.CreateSpecificCulture(language);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static List<string> GetPersonTobacoBrand(this Person person,SmartHookahContext db)
        {
            if (person != null)
            {
                if (person.MyTobacco)
                {
                    var tobacco = person.Tobacco.Select(b => b.BrandName).Distinct().ToList();
                    tobacco.Add(Smoke.SmokeSession.myMixes);
                    return tobacco;
                }
                else
                {
                   return db.Brands.Where(a => a.Tobacco).Select(a => a.Name).Distinct().ToList();
                }
            }
            return db.Brands.Where(a => a.Tobacco).Select(a => a.Name).Distinct().ToList();
        }

        public static DateTime ConvertTimeFromUtc(DateTime timeUtc)
        {
            DateTime destTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, TimeZoneInfo.Local);
            return destTime;
        }

        public static string Content(this UrlHelper urlHelper, string contentPath, bool toAbsolute = false)
        {
            var path = urlHelper.Content(contentPath);
            var url = new Uri(HttpContext.Current.Request.Url, path);

            return toAbsolute ? url.AbsoluteUri : path;
        }

        public static string ToFriendlyString(this PufType me)
        {
            switch (me)
            {
                case PufType.In:
                    return "Intake";
                case PufType.Out:
                    return "OutTake";
                case PufType.Idle:
                    return "Idle";



            }
            return me.ToString();
        }

        public static IEnumerable<TimeSpan> GetDuration(this List<Puf> pufs, Predicate<Puf> predicate)
        {
            
            for (int i = 1; i < pufs.Count(); i++)
            {
                if (predicate(pufs[i-1]))
                {
                    var t = TimeSpan.FromTicks((long) ((pufs[i ].Milis - pufs[i-1].Milis)*10000));

                    if (predicate(pufs[i-1]) == predicate(pufs[i]))
                    {
                        
                    }
                        if ( t > new TimeSpan())
                    yield return TimeSpan.FromTicks((long)((pufs[i].Milis - pufs[i-1].Milis) * 10000));
                    else
                    {
                        yield return new TimeSpan();
                    }
                

                }
            }
        }

        public static SelectList ToSelectedList(this List<Update> updates, Func<Update,string>  id,Func<Update,string> text  ) 
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (Update update in updates.OrderByDescending(a => a.ReleseDate))
            {
                result.Add(new SelectListItem() { Value = id(update), Text = text(update) });
            }
            return new SelectList(result, "Value", "Text");
        }

        public static IEnumerable<CPuf> GetClusterPuf(this List<DbPuf> pufs)
        {
            var cluster = false;
            for (int i = 0; i < pufs.Count() - 1; i++)
            {
                if(pufs[i].Type != PufType.Idle)
                { 
                var t = TimeSpan.FromTicks((long)((pufs[i + 1].Milis - pufs[i].Milis) * 10000));
                    if (t < new TimeSpan())
                    {
                        t = new TimeSpan();
                    }
                    var presure = pufs[i + 1].Presure;

                    if(presure < 0 && presure > -500) 
                        continue;

                    if (presure < 0)
                        cluster = !cluster;

                    yield return new CPuf()
                    {
                        Presure =  presure,
                        Duration = t,
                        TimeStamp =  pufs[i].DateTime,
                        Cluster = cluster? 0 : 1
                    };
                }
            }
        }
        

        public static string ToWebStateString(this PufType me)
        {
            switch (me)
            {
                case PufType.In:
                    return Smoke.SmokeSession.smoking;
                case PufType.Out:
                    return Smoke.SmokeSession.blowing;
                case PufType.Idle:
                    return Smoke.SmokeSession.waiting;
            }
            return me.ToString();
        }

        public static int ToGraphData(this PufType me)
        {

            switch (me)
            {
                case PufType.In:
                    return 1;
                case PufType.Out:
                    return -1;
                case PufType.Idle:
                    return 0;
            }
            return 0;
        }

        public static List<int> Bucketize(this IEnumerable<TimeSpan> source, int totalBuckets,out int bucketSize)
        {
            var min = source.Min(a => a.TotalMilliseconds);
            var max = source.Max(a => a.TotalMilliseconds);
            var buckets = new int[totalBuckets];

            bucketSize = (int)Math.Round((max - min) / totalBuckets);
            foreach (var value in source)
            {
                int bucketIndex = 0;
                if (bucketSize > 0)
                {
                    bucketIndex = (int)((value.TotalMilliseconds - min) / bucketSize);
                    if (bucketIndex == totalBuckets)
                    {
                        bucketIndex--;
                    }
                }
                buckets[bucketIndex]++;
            }
            return buckets.ToList();
        }

        public static int ToShortInt(this DateTime time)
        {
           return int.Parse(time.ToString("HHmm"));
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }


        public static int ToShortInt(this TimeSpan span)
        {
            var s = $"{span.Hours}{span.Minutes.ToString("00")}";
            return int.Parse(s);
        }


    }

    public static class TimeSpanExtension
    {
        /// <summary>
        /// Multiplies a timespan by an integer value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, int multiplier)
        {
            return TimeSpan.FromTicks(multiplicand.Ticks * multiplier);
        }

        /// <summary>
        /// Multiplies a timespan by a double value
        /// </summary>
        public static TimeSpan Multiply(this TimeSpan multiplicand, double multiplier)
        {
            return TimeSpan.FromTicks((long)(multiplicand.Ticks * multiplier));
        }
    }

    public class CPuf
    {
        public TimeSpan Duration { get; set; }

        public int Presure { get; set; }

        public DateTime TimeStamp { get; set; }

        public int Cluster { get; set; }
    }
}