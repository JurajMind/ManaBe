using System.Collections.Generic;

namespace smartHookah.Controllers
{
    public class EditOpenHoursPostModel
    {
        public int Id { get; set; }
        public List<Hours> BusinessHours { get; set; }
    }

}