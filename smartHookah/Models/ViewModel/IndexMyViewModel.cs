﻿using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    using System.Collections.Generic;

    public class IndexMyViewModel
    {
        public List<Hookah> Hookah { get; set; }
        public List<string> Online { get; set; }
    }
}