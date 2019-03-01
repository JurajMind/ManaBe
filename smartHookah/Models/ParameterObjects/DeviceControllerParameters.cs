using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using smartHookah.Models.Db;

namespace smartHookah.Models.ParameterObjects
{
    public class ChangeAnimation
    {
        public int AnimationId { get; set; }
        public PufType Type { get; set; }
    }

    public class ChangeSpeed
    {
        [Range(0,255)]
        public int Speed { get; set; }
        public PufType Type { get; set; }
    }

    public class ChangeColor
    {
        public Color Color { get; set; }
        public PufType Type { get; set; }
    }

    public class ChangeBrightness
    {
        [Range(0, 255)]
        public int Brightness { get; set; }
        public PufType Type { get; set; }
    }
}