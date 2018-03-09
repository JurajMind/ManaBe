using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace smartHookah.Models
{
    public class Place
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string LogoPath { get; set; }

        [MaxLength(255)]
        public string ShortDescriptions { get; set; }

        public string Descriptions { get; set; }

        [Required]
        [MaxLength(25)]
        [Index(IsUnique =true)]        
        public string FriendlyUrl { get; set; }

        public int AddressId { get; set; }

        public virtual Address Address { get; set; }

        public virtual ICollection<BusinessHours> BusinessHours { get; set; }

        public int? PersonId { get; set; }

        public virtual Person Person { get; set; }

        public string PhoneNumber { get; set; }

        public string Facebook { get; set; }

        public decimal BaseHookahPrice { get; set; }
        [MaxLength(3)]
        public string Currency { get; set; }
        
        public virtual ICollection<HookahOrder> Orders { get; set; }

        public virtual ICollection<OrderExtra> OrderExtras { get; set; }

        public virtual ICollection<Seat> Seats { get; set; }

        public virtual ICollection<Person> Managers { get; set; }

        public virtual ICollection<SmokeSession> SmokeSessions { get; set; }

        public virtual ICollection<Media> Medias { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        public  bool AllowReservation { get; set; }

        public int MinimumReservationTime { get; set; } 
        public string Map { get; set; }

        public bool Public { get; set; }

        public virtual int? FranchiseId { get; set; }

        public virtual Franchise Franchise { get; set; }

        public virtual ICollection<PriceGroup> PriceGroups { get; set; }

        public bool IsOpen(DateTime Open)
        {
            var todayInt = (int)Open.DayOfWeek;
            var todayOpenHours =this.BusinessHours.FirstOrDefault(a => a.Day == todayInt);

            if (todayOpenHours == null)
                return false;

            if (todayOpenHours.OpenTine < Open.TimeOfDay && todayOpenHours.CloseTime < Open.TimeOfDay)
                return true;

            // Is before open, check day before close time
            if (todayOpenHours.OpenTine < Open.TimeOfDay)
            {
                var lastDayint = (int)Open.AddDays(-1).DayOfWeek;
               var lastDayOpenHour = this.BusinessHours.FirstOrDefault(a => a.Day == todayInt);

                if (lastDayOpenHour == null)
                    return false;

                if (lastDayOpenHour.CloseTime > Open.TimeOfDay)
                    return true;
            }

            if (todayOpenHours.CloseTime > Open.TimeOfDay)
            {
                var lastDayint = (int)Open.AddDays(1).DayOfWeek;
                var lastDayOpenHour = this.BusinessHours.FirstOrDefault(a => a.Day == todayInt);

                if (lastDayOpenHour == null)
                    return false;

                if (lastDayOpenHour.OpenTine > Open.TimeOfDay)
                    return true;
            }

            return false;
        }


    }


    public class Seat
    {
        public int Id { get; set; }
        [MaxLength(64)]
        public string Name { get; set; }
        public int? PlaceId { get; set; }
        public virtual Place Place { get; set; }

        [MaxLength(5)]
        public string Code { get; set; } 

        public int Capacity { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }
    }

    public class HookahOrder
    {
        public DateTime Created { get; set; }
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        public int? PersonId { get; set; }
        public virtual Person Person { get; set; }

        public int SmokeSessionMetaDataId { get; set; }

        public virtual SmokeSessionMetaData SmokeSessionMetaData { get; set; }

        
        public int? SmokeSessionId { get; set; }
        public virtual SmokeSession SmokeSession { get; set; }
        
        [MaxLength(255)]
        public string ExtraInfo { get; set; }
        
        public OrderState State { get; set; }

        public decimal Price { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "CZK";

        public int? SeatId { get; set; }

        public virtual Seat Seat { get; set; }

        public int? ReservationId { get; set; }
        public virtual Reservation Reservation { get; set; }

        public HookahOrderType Type { get; set; }
    }

    public enum HookahOrderType
    {
       Before = 0,
       InPlace = 1,
       Staff = 2,
    }

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

        public virtual ICollection<Seat> Seats { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }
        [NotMapped]
        public string DisplayName {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                    return Name;
                if (Person == null)
                    return Name;

                return Person.User.First().Email;
            } }

        public virtual ICollection<Person> Customers { get; set; }

        public string getEmail()
        {
            //Check if reservation is not created by manager
            if(this.Place.Managers.Any(a => a.Id == this.PersonId))
            {
                return this.Person.User.First().Email;
                return null;
            }
           return this.Person.User.First().Email;
        }
    }

    public enum ReservationState
    {
        Created = 0,
        Canceled = 1,
        Denied = 2,
        Confirmed = 3,
        VisitConfirmed = 4,
        UnConfirmed = 5,
        ConfirmationRequired = 6,
        NonVisit = 7,
    }



    public enum OrderState
    {
        Open = 0,
        Processing = 1,
        Ready = 2,
        Delivered = 3,
        Canceled =4,
    }

    public class BusinessHours
    {
        public int Id { get; set; }
        public int LoungeId { get; set; }

        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        public int Day { get; set; }

        public TimeSpan OpenTine { get; set; }

        public TimeSpan CloseTime { get; set; }
    }


    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Number { get; set; }

        public string ZIP { get; set; }
        [MaxLength(10)]
        public  string Lat { get; set; }
        [MaxLength(10)]
       public string Lng { get; set; }

   
        public override string ToString()
        {
            return $"{Street} {Number} {City} {ZIP}";
        }
    }

    public class Media
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Path { get; set; }

        public MediaType Type { get; set; }
        
        [NotMapped]
        public string Extension {
            get
            {
              return System.IO.Path.GetExtension(Path);
             
            } }

        [NotMapped]
        public string FileName
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(Path); }
        }

        [NotMapped]
        public string GetDirectory
        {
            get { return System.IO.Path.GetDirectoryName(Path); }
        }


        public string GetSize(int i)
        {
            var a = Path.ToString();
            var LastDot = a.LastIndexOf(".");
            return a.Insert(LastDot,"."+ i.ToString());
        }
    }

    public enum MediaType
    {
        Picture = 0,
        Video = 1,
    }

    public class Franchise
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Uril { get; set; }

        public virtual ICollection<Place> Places { get; set; }
    }
}