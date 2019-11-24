using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;
using System.Linq;

namespace smartHookah.Models.Db.Place
{
    public class Place
    {
        [Key] public int Id { get; set; }

        [Required] [MaxLength(255)] public string Name { get; set; }

        public string LogoPath { get; set; }

        [MaxLength(255)] public string ShortDescriptions { get; set; }

        public string Descriptions { get; set; }

        public virtual ICollection<PlaceTranslation> DbDescription { get; set; }


        [Required]
        [MaxLength(25)]
        [Index(IsUnique = true)]
        public string FriendlyUrl { get; set; }

        public int AddressId { get; set; }

        public virtual Address Address { get; set; }

        public virtual ICollection<BusinessHours> BusinessHours { get; set; }

        public virtual ICollection<PlaceDay> PlaceDays { get; set; }

        public virtual ICollection<SocialMedia> SocialMedias { get; set; }

        public int? PersonId { get; set; }

        public virtual Person Person { get; set; }

        public int? CreatorId { get; set; }

        public virtual Person Creator { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string PhoneNumber { get; set; }

        public string Facebook { get; set; }

        public string Url { get; set; }

        public decimal BaseHookahPrice { get; set; }
        [MaxLength(3)] public string Currency { get; set; }

        public virtual ICollection<HookahOrder> Orders { get; set; }

        public virtual ICollection<OrderExtra> OrderExtras { get; set; }

        public virtual ICollection<Seat> Seats { get; set; }

        public virtual ICollection<Person> Managers { get; set; }

        public virtual ICollection<SmokeSession> SmokeSessions { get; set; }

        public virtual ICollection<Media> Medias { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        public bool HaveReservation { get; set; }

        public bool HaveMenu { get; set; }

        public bool HaveOrders { get; set; }

        public bool HaveMana { get; set; }

        public int Rating { get; set; }

        public ICollection<PlaceReview> Reviews { get; set; }

        public int MinimumReservationTime { get; set; }
        public string Map { get; set; }

        public bool Public { get; set; }

        public PlaceSrc Src { get; set; }

        public PlaceState State { get; set; }

        public virtual ICollection<PlaceFlag> PlaceFlags { get; set; }

        public virtual int? FranchiseId { get; set; }

        public virtual Franchise Franchise { get; set; }

        public virtual ICollection<PriceGroup> PriceGroups { get; set; }

        public bool IsOpen(DateTime open)
        {
            var todayInt = (int)open.DayOfWeek;
            var todayOpenHours = BusinessHours?.FirstOrDefault(a => a.Day == todayInt);

            if (todayOpenHours == null)
                return false;

            if (todayOpenHours.OpenTine < open.TimeOfDay && todayOpenHours.CloseTime < open.TimeOfDay)
                return true;

            // Is before open, check day before close time
            if (todayOpenHours.OpenTine < open.TimeOfDay)
            {
                var lastDayOpenHour = this.BusinessHours.FirstOrDefault(a => a.Day == todayInt);

                if (lastDayOpenHour == null)
                    return false;

                if (lastDayOpenHour.CloseTime > open.TimeOfDay)
                    return true;
            }

            if (todayOpenHours.CloseTime > open.TimeOfDay)
            {
                var lastDayOpenHour = this.BusinessHours.FirstOrDefault(a => a.Day == todayInt);

                if (lastDayOpenHour == null)
                    return false;

                if (lastDayOpenHour.OpenTine > open.TimeOfDay)
                    return true;
            }

            return false;
        }
    }

    public enum PlaceSrc
    {
        Import,
        Person,
        Place
    }

    public enum PlaceState
    {
        Active = 0,
        Blocked = 2,
        Waiting = 1,
    }

    public class PlaceFlag
    {
        public int Id { get; set; }

        public virtual ICollection<Place> Places { get; set; }

        public string Code { get; set; }
    }

    public class Seat
    {
        public int Id { get; set; }
        [MaxLength(64)] public string Name { get; set; }
        public int? PlaceId { get; set; }
        public virtual Place Place { get; set; }

        [MaxLength(5)] public string Code { get; set; }

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

        [MaxLength(255)] public string ExtraInfo { get; set; }

        public OrderState State { get; set; }

        public decimal Price { get; set; }

        [MaxLength(3)] public string Currency { get; set; } = "CZK";

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

    public enum ReservationState
    {
        Created = 0,
        Canceled = 1,
        Denied = 2,
        Confirmed = 3,
        Visited = 4,
        UnConfirmed = 5,
        ConfirmationRequired = 6,
        NonVisited = 7,
    }

    public enum ReservationSrc
    {
        Manualy = 0,
        Web = 1,
        App = 2,
    }


    public enum OrderState
    {
        Open = 0,
        Processing = 1,
        Ready = 2,
        Delivered = 3,
        Canceled = 4,
    }

    public class BusinessHours
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }
        public virtual Place Place { get; set; }

        public int Day { get; set; }

        public TimeSpan OpenTine { get; set; }

        public TimeSpan CloseTime { get; set; }
    }


    public class Address
    {
        [Key] public int Id { get; set; }

        public int AddressId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Number { get; set; }

        public string ZIP { get; set; }
        public string Country { get; set; }

        [MaxLength(10)] public string Lat { get; set; }
        [MaxLength(10)] public string Lng { get; set; }

        public DbGeography Location { get; set; }


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

        public string Sizes { get; set; }
        public MediaType Type { get; set; }
        public bool IsDefault { get; set; }

        [NotMapped] public string Extension => System.IO.Path.GetExtension(Path);

        [NotMapped] public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

        [NotMapped] public string GetDirectory => System.IO.Path.GetDirectoryName(Path);

        public virtual PlaceReview PlaceReview { get; set; }

        public virtual SessionReview SessionReview { get; set; }

        public virtual Place Place { get; set; }

        public virtual PipeAccessoryReview PipeAccessoryReview { get; set; }

        public string Sized(int i = 0)
        {
            if (i == 0) return Path + "original.jpg";
            else return Path + $"{i}.jpg";
        }

        public string GetSize(int i)
        {
            return this.Sized(i);
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