using smartHookah.Models.Db.Place;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace smartHookah.Models.Db
{
    public class Person
    {
        private bool _isPlace;

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<SmokeSession> SmokeSessions { get; set; }

        public virtual ICollection<OwnPipeAccesories> OwnedPipeAccesories { get; set; }
        public virtual ICollection<Hookah> Hookahs { get; set; }
        public virtual ICollection<PlaceEvent> PlaceEvents { get; set; }

        [DefaultValue("True")]
        public bool AutoAssign { get; set; }

        [DefaultValue("true")]
        public bool MyGear { get; set; }

        [DefaultValue("true")]
        public bool MyTobacco { get; set; }

        [ForeignKey("AssignedBrand")]
        public string AssignedBrandId { get; set; }

        public Brand AssignedBrand { get; set; }

        public virtual ICollection<Place.Place> Places { get; set; }

        [NotMapped]
        public Place.Place Place
        {
            get { return Places.FirstOrDefault(); }
        }

        public virtual int? FeatureMixCreatorId { get; set; }
        public virtual FeatureMixCreator FeatureMixCreator { get; set; }

        [NotMapped]
        public bool IsPlace
        {
            get { return Places.Count > 0; }
        }

        public virtual ICollection<HookahOrder> Orders { get; set; }

        public virtual ICollection<Place.Place> Manage { get; set; }

        public virtual ICollection<Notificatiom> Notificatioms { get; set; }

        public virtual GameProfile GameProfile { get; set; }

        public virtual int? PresetId { get; set; }
        public virtual SmokeSessionMetaData Preset { get; set; }

        public virtual GamePerson Game { get; set; }

        public virtual DeviceSetting DefaultSetting => this.DefaultPreset?.DeviceSetting;

        public int? DefaultPresetId { get; set; }
        public virtual DevicePreset DefaultPreset { get; set; }

        public virtual ICollection<DevicePreset> Presets { get; set; }

        public int? DefaultMetaDataId { get; set; }
        public virtual SmokeSessionMetaData DefaultMetaData { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        public virtual ICollection<Reservation> InReservations { get; set; }

        public virtual ICollection<ApplicationUser> User { get; set; }

        public virtual PersonStatistic Statistic { get; set; }

        public virtual ICollection<NotificationToken> NotificationTokens { get; set; }

        public virtual ICollection<FeatureMixCreator> FollowedMixCreators { get; set; }

        public virtual ICollection<Friendship> Friends { get; set; }

        public bool Gdpr { get; set; }

        public int PersonRating { get; set; }

        public virtual ICollection<PipeAccesoryLike> Likes { get; set; }

        [NotMapped]
        public virtual IEnumerable<TobaccoMix> FavoriteTobaccoMixs
        {
            get
            {
                return this.Likes.Where(a => a.PipeAccesory is TobaccoMix && a.Value > 0)
                    .Select(a => a.PipeAccesory as TobaccoMix);
            }
        }

        [NotMapped]
        public virtual IEnumerable<Pipe> Pipes
        {
            get
            {
                return
                    OwnedPipeAccesories.Where(a => a.PipeAccesory is Pipe && !a.DeleteDate.HasValue)
                        .Select(a => a.PipeAccesory)
                        .Cast<Pipe>();
            }
        }

        [NotMapped]
        public virtual IEnumerable<Bowl> Bowls
        {
            get
            {
                return
                    OwnedPipeAccesories.Where(a => a.PipeAccesory is Bowl && !a.DeleteDate.HasValue)
                        .Select(a => a.PipeAccesory)
                        .Cast<Bowl>()
                        .ToList();
            }
        }

        [NotMapped]
        public virtual IEnumerable<Tobacco> Tobaccos
        {
            get
            {
                return
                    OwnedPipeAccesories.Where(a => a.PipeAccesory is Tobacco && !a.DeleteDate.HasValue)
                        .Select(a => a.PipeAccesory)
                        .Cast<Tobacco>();
            }
        }

        [NotMapped]
        public virtual IEnumerable<HeatManagment> HeatManagments
        {
            get
            {
                return
                    OwnedPipeAccesories.Where(a => a.PipeAccesory is HeatManagment && !a.DeleteDate.HasValue)
                        .Select(a => a.PipeAccesory)
                        .Cast<HeatManagment>();
            }
        }

        [NotMapped]
        public virtual IEnumerable<Coal> Coals
        {
            get
            {
                return
                    OwnedPipeAccesories.Where(a => a.PipeAccesory is Coal && !a.DeleteDate.HasValue)
                        .Select(a => a.PipeAccesory)
                        .Cast<Coal>();
            }
        }



        [NotMapped]
        public string DisplayName
        {
            get
            {
                var displayName = User.First().DisplayName;
                if (string.IsNullOrEmpty(displayName))
                    return User.First().Email;

                return displayName;
            }
        }

        public static Person CreateDefault()
        {
            var person = new Person();
            person.GameProfile = new GameProfile();
            return person;
        }



    }

    public class GamePerson
    {
        public int Id { get; set; }

        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        public int Value { get; set; }
    }

    public class OwnPipeAccesories
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        [NotMapped]
        public bool Deleted
        {
            get { return DeleteDate.HasValue; }
        }

        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        public int PipeAccesoryId { get; set; }

        public virtual PipeAccesory PipeAccesory { get; set; }

        public decimal Amount { get; set; }

        public bool Curent { get; set; }

        public string AlternativeName { get; set; }

        public virtual ICollection<PriceGroupPrice> Prices { get; set; }

        public string Currency { get; set; }
        [NotMapped]
        public decimal Price
        {
            get { return 0; }
        }
    }
}
