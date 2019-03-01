using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Controllers
{
    public class PlaceDetailsViewModel
    {
        public Place Place { get; set; }
        public bool CanEdit { get; set; }
    }

}