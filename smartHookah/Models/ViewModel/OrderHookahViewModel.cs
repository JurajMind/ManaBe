using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using System.Collections.Generic;

namespace smartHookah.Controllers
{
    public class OrderHookahViewModel
    {
        public Place Place { get; set; }
        public List<Pipe> Hookah { get; set; }
        public List<Bowl> Bowls { get; set; }
        public List<Tobacco> Tobacco { get; set; }
        public ICollection<Seat> Seat { get; set; }
        public List<Seat> AllSeat { get; set; }
        public Reservation Reservation { get; set; }
        public bool CanOrder { get; set; } = true;
    }

}