using System.Collections.Generic;
using smartHookah.Models;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;

namespace smartHookah.Controllers
{
    public class EditOpenHoursViewModel
    {
        public Place Place { get; set; }
        public List<BusinessHours> BusinessHours { get; set; }
    }

}