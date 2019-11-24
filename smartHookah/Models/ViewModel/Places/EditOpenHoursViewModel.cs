using smartHookah.Models.Db.Place;
using System.Collections.Generic;

namespace smartHookah.Controllers
{
    public class EditOpenHoursViewModel
    {
        public Place Place { get; set; }
        public List<BusinessHours> BusinessHours { get; set; }
    }

}