using smartHookah.Models.Db.Place;

namespace smartHookah.Controllers
{
    public class PlaceDetailsViewModel
    {
        public Place Place { get; set; }
        public bool CanEdit { get; set; }
    }

}