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
            result.Add(new Animation(0, "Off", AnimationUsage.All,haveColor:false));
            result.Add(new Animation(1, "Puff Bar", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(2, "Flicker", AnimationUsage.All));
            result.Add(new Animation(3, "OneColor", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(4, "Rainbow", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(5, "Intense Bar", AnimationUsage.Action, haveColor: false));
            result.Add(new Animation(6, "Intense Breathe", AnimationUsage.Action));
            result.Add(new Animation(7, "Selected Color", AnimationUsage.All));
            result.Add(new Animation(8, "Fade", AnimationUsage.All));
            result.Add(new Animation(9, "Mood Circles", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(10, "Random Burst", AnimationUsage.All));
            result.Add(new Animation(11, "Bounce", AnimationUsage.All));
            result.Add(new Animation(12, "Bounce Fade", AnimationUsage.All));
            result.Add(new Animation(13, "Infinite Chase", AnimationUsage.All));
            result.Add(new Animation(14, "Dots Chase", AnimationUsage.All));
            result.Add(new Animation(15, "Flicker2", AnimationUsage.All));
            result.Add(new Animation(16, "Breathe", AnimationUsage.All));
            result.Add(new Animation(17, "Re Breathe", AnimationUsage.All));
            result.Add(new Animation(18, "Fade Vertical", AnimationUsage.All));
            result.Add(new Animation(19, "Rule 30", AnimationUsage.Action));
            result.Add(new Animation(20, "Random March", AnimationUsage.Action));
            result.Add(new Animation(21, "Tricolor", AnimationUsage.All));
            result.Add(new Animation(22, "Radiation", AnimationUsage.All));
            result.Add(new Animation(23, "Color Loop", AnimationUsage.All));
            result.Add(new Animation(24, "Pop", AnimationUsage.All));
            result.Add(new Animation(25, "Intense Color", AnimationUsage.Action));
            result.Add(new Animation(26, "Flame", AnimationUsage.All));
            result.Add(new Animation(27, "Rainbow March", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(28, "Pac-man", AnimationUsage.All, haveColor: false));
            result.Add(new Animation(29, "Random Pop", AnimationUsage.All));
            result.Add(new Animation(30, "Strobo", AnimationUsage.All));
            result.Add(new Animation(31, "Three way", AnimationUsage.All));
            result.Add(new Animation(32, "Kitt", AnimationUsage.All));
            result.Add(new Animation(33, "Matrix", AnimationUsage.Action));
            result.Add(new Animation(34, "Different rainbow", AnimationUsage.Action, haveColor: false));

            result.Add(new Animation(35, "Session Progress", AnimationUsage.Idle, versionFrom: 1000020));
            result.Add(new Animation(36, "Puff Time", AnimationUsage.Action,versionFrom: 1000020));

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
            int versionTo = 2000000, bool haveColor = true)
        {
            Id = id;
            DisplayName = dispayName;
            VersionFrom = versionFrom;
            VersionTo = versionTo;
            Usage = usage;
            HaveColor = haveColor;
        }

        public int Id { get; set; }
        public string DisplayName { get; set; }

        public int VersionFrom { get; set; }

        public int VersionTo { get; set; }

        public AnimationUsage Usage { get; set; }

        public bool HaveColor { get; set; }
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