using System;
using System.Linq;

namespace smartHookah.Models.Dto
{
    using System.Collections.Generic;

    public class HookahSimpleDto
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsOnline { get; set; }

        public static HookahSimpleDto FromModel(Hookah modelHookah)
        {
            return new HookahSimpleDto()
                       {
                           Code = modelHookah.Code,
                           Name = modelHookah.Name
                       };
        }
    }
}