using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace smartHookah.Models.Db
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }
        public int PlaceId { get; set; }
        public virtual  Place Place { get; set; }
        
        public DateTime? Started { get; set; }

        public DateTime? End { get; set; }

        public ReservationState Status { get; set; }

        public virtual ICollection<HookahOrder> Orders { get; set; }
        
        public int Persons { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime Time { get; set; }

        public int? Late { get; set; }

        public ReservationSrc Src { get; set; }

        public virtual ICollection<Seat> Seats { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }
        [NotMapped]
        public string DisplayName {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                    return Name;
                return Person == null ? Name : Person.User.First().Email;
            } }

        public virtual ICollection<Person> Customers { get; set; }

        public string getEmail()
        {
            //Check if reservation is not created by manager
            return this.Place.Managers.Any(a => a.Id == this.PersonId) ? null : this.Person.User.First().Email;
        }
    }
}