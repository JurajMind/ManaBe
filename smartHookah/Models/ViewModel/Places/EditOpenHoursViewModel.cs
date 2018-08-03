using System.Collections.Generic;
using smartHookah.Models;

namespace smartHookah.Controllers
{
    public class EditOpenHoursViewModel
    {
        public Place Place { get; set; }
        public List<BusinessHours> BusinessHours { get; set; }
    }

}