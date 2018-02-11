using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace smartHookah.Helpers
{
    public static class AnimationHelper
    {
        public static List<Animation> Animations = CreateAnimations();


        private static List<Animation> CreateAnimations()
        {
            var result = new List<Animation>();
            result.Add(new Animation(0, "Off", AnimationUsage.All));
            result.Add(new Animation(1, "SmokeBar", AnimationUsage.All));
            result.Add(new Animation(2, "Flicker", AnimationUsage.All));
            result.Add(new Animation(3, "OneColor", AnimationUsage.All));
            result.Add(new Animation(4, "Rainbow", AnimationUsage.All));
            result.Add(new Animation(5, "PresureBar", AnimationUsage.Action));
            result.Add(new Animation(6, "PresureBreath", AnimationUsage.Action));
            result.Add(new Animation(7, "SelectedColor", AnimationUsage.All));
            result.Add(new Animation(8, "RainbowFade", AnimationUsage.All));
            result.Add(new Animation(9, "RainbowLoop", AnimationUsage.All));
            result.Add(new Animation(10, "RandomBurst", AnimationUsage.All));
            result.Add(new Animation(11, "ColorBounce", AnimationUsage.All));
            result.Add(new Animation(12, "ColorBounceFade", AnimationUsage.All));
            result.Add(new Animation(13, "EmsLightOne", AnimationUsage.All));
            result.Add(new Animation(14, "EmsLightAll", AnimationUsage.All));
            result.Add(new Animation(15, "Flicker2", AnimationUsage.All));
            result.Add(new Animation(16, "Breath", AnimationUsage.All));
            result.Add(new Animation(17, "BreathInverse", AnimationUsage.All));
            result.Add(new Animation(18, "FadeVertical", AnimationUsage.All));
            result.Add(new Animation(19, "Rule30", AnimationUsage.Action));
            result.Add(new Animation(20, "RandomMarch", AnimationUsage.Action));
            result.Add(new Animation(21, "RwbMarch", AnimationUsage.All));
            result.Add(new Animation(22, "Radiation", AnimationUsage.All));
            result.Add(new Animation(23, "ColorLoop", AnimationUsage.All));
            result.Add(new Animation(24, "Pop", AnimationUsage.All));
            result.Add(new Animation(25, "PresureColor", AnimationUsage.Action));
            result.Add(new Animation(26, "Flame", AnimationUsage.All));
            result.Add(new Animation(27, "RainbowVertica", AnimationUsage.All));
            result.Add(new Animation(28, "Pacman", AnimationUsage.All));
            result.Add(new Animation(29, "RandomColorPop", AnimationUsage.All));
            result.Add(new Animation(30, "EmsStrobo", AnimationUsage.All));
            result.Add(new Animation(31, "RgbPropeller", AnimationUsage.All));
            result.Add(new Animation(32, "Kitt", AnimationUsage.All));
            result.Add(new Animation(33, "Matrix", AnimationUsage.Action));
            result.Add(new Animation(34, "NewRainbow", AnimationUsage.Action));

            result.Add(new Animation(35, "SessionProgress", AnimationUsage.Idle, versionFrom: 1000020));
            result.Add(new Animation(36, "SmokeTimeBar", AnimationUsage.Action,versionFrom: 1000020));

            return result;
        }

        public static MvcHtmlString AnimationDropdown(this HtmlHelper helper,
            int animation,int version, string classname = "",string id="",string onChange="",
            IDictionary<string, object> htmlAttributes = null)
        {
            var attributes = new StringBuilder();
            if (htmlAttributes != null)
                foreach (var htmlAttribute in htmlAttributes)
                    attributes.Append(htmlAttribute);

            var sb = new StringBuilder();
            sb.AppendFormat("<select class=\"{0}\" id=\"{1}\" onchange=\"{2}\" " + attributes + ">",classname,id,onChange);
            foreach (
                var item in
                Animations.Where(a => a.VersionFrom <= version && a.VersionTo >= version).OrderBy(a => a.DisplayName))
            {
                var selected = "";
                if (item.Id == animation)
                {
                    selected = "selected=\"selected\"";
                }
                sb.AppendFormat("<option value=\"{0}\" {2}>{1}</option>", item.Id, item.DisplayName,selected);
            }
               
            sb.AppendLine("</select>");

            return MvcHtmlString.Create(sb.ToString());
        }
    }

    public struct Animation
    {
        public Animation(int id, string dispayName, AnimationUsage usage, int versionFrom = 1000000,
            int versionto = 2000000)
        {
            Id = id;
            DisplayName = dispayName;
            VersionFrom = versionFrom;
            VersionTo = versionto;
            Usage = usage;
        }

        public int Id { get; set; }
        public string DisplayName { get; set; }

        public int VersionFrom { get; set; }

        public int VersionTo { get; set; }

        public AnimationUsage Usage { get; set; }
    }

    [Flags]
    public enum AnimationUsage
    {
        None = 0,
        Puf = 1,
        Blow = 2,
        Idle = 4,
        Action = 6,
        All = 7
    }
}