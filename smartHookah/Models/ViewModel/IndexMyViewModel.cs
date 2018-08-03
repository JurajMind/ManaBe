namespace smartHookah.Controllers
{
    using System.Collections.Generic;

    using smartHookah.Models;

    public class IndexMyViewModel
    {
        public List<Hookah> Hookah { get; set; }
        public List<string> Online { get; set; }
    }
}