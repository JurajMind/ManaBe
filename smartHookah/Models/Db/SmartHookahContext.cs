using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using smartHookah.Models.Db.Device;
using smartHookah.Models.Db.Gear;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Db.Session;

namespace smartHookah.Models.Db
{
    public class SmartHookahContext : IdentityDbContext<ApplicationUser>
    {
        public SmartHookahContext()
            : base("SmartHookah")
        {
#if DEBUG
            this.Database.Log = Console.WriteLine;
#endif
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Bowl> Bowls { get; set; }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<BusinessHours> BusinessHours { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<DbPuf> DbPufs { get; set; }

        public DbSet<EventProgress> EventProgresses { get; set; }

        public DbSet<EventRecepie> EventRecepies { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<GameProfile> GameProfiles { get; set; }

        public DbSet<HookahOrder> HookahOrders { get; set; }

        public virtual DbSet<Hookah> Hookahs { get; set; }

        public virtual DbSet<DeviceSetting> HookahSettings { get; set; }

        public virtual DbSet<DevicePreset> DevicePreset { get; set; }

        public DbSet<Media> Media { get; set; }

        public DbSet<OrderExtra> OrderExtras { get; set; }

        public DbSet<OwnPipeAccesories> OwnPipeAccesorieses { get; set; }

        public virtual DbSet<Person> Persons { get; set; }

        public DbSet<PipeAccesory> PipeAccesories { get; set; }

        public DbSet<PipeAccesoryLike> PipeAccesoryLikes { get; set; }

        public DbSet<PipeAccesoryStatistics> PipeAccesoryStatistics { get; set; }

        public DbSet<Pipe> Pipes { get; set; }

        public DbSet<HeatManagment> HeatManagments { get; set; }

        public DbSet<Coal> Coals { get; set; }


        public DbSet<PlaceDay> PlaceDays { get; set; }

        public DbSet<PlaceEvent> PlaceEvents { get; set; }

        public DbSet<Place.Place> Places { get; set; }

        public DbSet<PriceGroup> PriceGroup { get; set; }

        public DbSet<PriceGroupPrice> PriceGroupPrice { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Reward> Rewards { get; set; }

        public DbSet<Seat> Seats { get; set; }

        public DbSet<SmokeSessionMetaData> SessionMetaDatas { get; set; }

        public DbSet<SmokeSessionStatistics> SessionStatistics { get; set; }

        public DbSet<SmokeSession> SmokeSessions { get; set; }

        public DbSet<StandPicture> StandPictures { get; set; }

        public DbSet<TobaccoMix> TobaccoMixs { get; set; }

        public DbSet<TobaccoReview> TobaccoReviews { get; set; }

        public DbSet<SessionReview> SessionReviews { get; set; }

        public DbSet<PlaceReview> PlaceReviews { get; set; }

        public DbSet<PipeAccessoryReview> PipeAccessoryReviews { get; set; }

        public DbSet<Tobacco> Tobaccos { get; set; }

        public DbSet<TobacoMixPart> TobaccosMixParts { get; set; }

        public DbSet<TobaccoTaste> TobaccoTastes { get; set; }

        public DbSet<UpdateLog> UpdateLogs { get; set; }

        public DbSet<Update> Updates { get; set; }

        public DbSet<PlaceFlag> PlaceFlags { get; set; }

        public DbSet<Franchise> Franchises { get; set; }

        public DbSet<FeatureMixCreator> FeatureMixCreators { get; set; }

        public DbSet<NotificationToken> NotificationTokens { get; set; }

        public static SmartHookahContext Create()
        {
            return new SmartHookahContext();
        }

        public SmokeSession GetCurrentSession(int id)
        {
            var hookah = this.Hookahs.Find(id);
            return this.CurrentSession(hookah);
        }

        public SmokeSession GetSmokeSession(string hookahCode)
        {
            var hookah = this.Hookahs.FirstOrDefault(h => h.Code == hookahCode);
            return this.CurrentSession(hookah);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.UserId, r.RoleId }).ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");

            modelBuilder.Entity<SmokeSession>().HasOptional(a => a.Statistics);

            modelBuilder.Entity<Person>().HasMany(s => s.SmokeSessions).WithMany(c => c.Persons).Map(
                cs =>
                    {
                        cs.MapLeftKey("SmokeSessionRefId");
                        cs.MapRightKey("PersonRefId");
                        cs.ToTable("SmokeSessionPerson");
                    });

            modelBuilder.Entity<Person>().HasMany(s => s.Hookahs).WithMany(h => h.Owners).Map(
                cs =>
                    {
                        cs.MapLeftKey("HookahRefId");
                        cs.MapRightKey("PersonRefId");
                        cs.ToTable("HookahOwning");
                    });

            modelBuilder.Entity<SmokeSessionMetaData>().HasMany(s => s.Tags).WithMany(h => h.Used).Map(
                cs =>
                    {
                        cs.MapLeftKey("SmokeSessionMetadataRefId");
                        cs.MapRightKey("TagRefId");
                        cs.ToTable("SmokeSessionMetadataTags");
                    });

            modelBuilder.Entity<Person>().HasMany(s => s.Manage).WithMany(h => h.Managers).Map(
                cs =>
                    {
                        cs.MapLeftKey("PersonRefId");
                        cs.MapRightKey("PlaceRefId");
                        cs.ToTable("PlaceManagers");
                    });

            modelBuilder.Entity<Tobacco>().HasMany(s => s.Tastes).WithMany(h => h.Tobaccos).Map(
                cs =>
                    {
                        cs.MapLeftKey("TobaccoRefId");
                        cs.MapRightKey("TasteRefId");
                        cs.ToTable("TobaccoTasteBinding");
                    });


            modelBuilder.Entity<FeatureMixCreator>().HasMany(s => s.Followers).WithMany(h => h.FollowedMixCreators).Map(
                cs =>
                {
                    cs.MapLeftKey("FeatureMixCreatorRefId");
                    cs.MapRightKey("PersonRefId");
                    cs.ToTable("FeatureMixCreatorFollow");
                });
            
            modelBuilder.Entity<Person>().HasMany(s => s.Friends).WithRequired(s => s.A).WillCascadeOnDelete(false);
            modelBuilder.Entity<Person>().HasMany(s => s.Friends).WithRequired(s => s.B).WillCascadeOnDelete(false);

            modelBuilder.Entity<FeatureMixCreator>().HasRequired(a => a.Person).WithOptional(a => a.FeatureMixCreator);

            modelBuilder.Entity<Person>().HasOptional(a => a.GameProfile).WithRequired(a => a.Person);

            modelBuilder.Entity<Person>().HasMany(s => s.OwnedPipeAccesories).WithRequired(a => a.Person);

            modelBuilder.Entity<Brand>().HasMany(s => s.PipeAccessories).WithRequired(a => a.Brand);

            modelBuilder.Entity<SmokeSession>().HasOptional(a => a.Review).WithOptionalDependent();

            modelBuilder.Entity<PipeAccesory>().HasOptional(a => a.Statistics).WithRequired(a => a.PipeAccesory);

            modelBuilder.Entity<Place.Place>().HasOptional(a => a.Person).WithMany().HasForeignKey(a => a.PersonId);

            modelBuilder.Entity<Person>().HasOptional(a => a.AssignedBrand);

            modelBuilder.Entity<SmokeSession>().HasMany(a => a.SmokeEvents).WithRequired(a => a.SmokeSession);

            modelBuilder.Entity<Place.Place>().HasMany(a => a.SmokeSessions).WithOptional(a => a.Place);

            modelBuilder.Entity<Person>().HasMany(a => a.Places).WithOptional(a => a.Person);

            modelBuilder.Entity<Person>().HasOptional(a => a.Game).WithRequired(a => a.Person);

            modelBuilder.Entity<TobaccoMix>().HasMany(a => a.Tobaccos).WithRequired(b => b.InMix).WillCascadeOnDelete(false);

            modelBuilder.Entity<Reservation>().HasMany(s => s.Customers).WithMany(h => h.InReservations).Map(
                cs =>
                    {
                        cs.MapLeftKey("ReservationRefId");
                        cs.MapRightKey("PersonRefId");
                        cs.ToTable("ReservationCustomers");
                    });

            modelBuilder.Entity<Reservation>().HasMany(s => s.Customers).WithMany(h => h.InReservations).Map(
                cs =>
                    {
                        cs.MapLeftKey("ReservationRefId");
                        cs.MapRightKey("PersonRefId");
                        cs.ToTable("ReservationCustomers");
                    });

            modelBuilder.Entity<Reservation>().HasMany(s => s.Seats).WithMany(h => h.Reservations).Map(
                cs =>
                    {
                        cs.MapLeftKey("ReservationRefId");
                        cs.MapRightKey("SeatRefId");
                        cs.ToTable("ReservationSeat");
                    });

            modelBuilder.Entity<Place.Place>().HasMany(s => s.PlaceFlags).WithMany(h => h.Places).Map(
                cs =>
                {
                    cs.MapLeftKey("PlaceRefId");
                    cs.MapRightKey("FlagRefId");
                    cs.ToTable("PlaceFlagMapping");
                });

            modelBuilder.Entity<PipeAccesory>().HasMany(s => s.ImportInformations).WithMany(h => h.ImportedAccesories).Map(
                cs =>
                {
                    cs.MapLeftKey("AccessoryRefId");
                    cs.MapRightKey("ImportRefId");
                    cs.ToTable("AccessoryImportMapping");
                });

            modelBuilder.Entity<OwnPipeAccesories>().HasMany(s => s.Prices).WithRequired(a => a.PipeAccesorie);

            modelBuilder.Entity<PipeAccesory>().HasMany(s => s.SimilarAccesories).WithRequired(h => h.Original)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Place.Place>().HasMany(p => p.PlaceDays);

            modelBuilder.Entity<PlaceDay>().HasMany(p => p.PlaceEvents);

            modelBuilder.Entity<PlaceEvent>().HasMany(p => p.Persons).WithMany(e => e.PlaceEvents);
            
            modelBuilder.Entity<Person>().HasOptional(d => d.DefaultPreset);

            modelBuilder.Entity<DevicePreset>().HasOptional(a => a.Person).WithMany(a => a.Presets)
                .HasForeignKey(a => a.PersonId);

            modelBuilder.Entity<Franchise>().HasMany(a => a.Places).WithOptional(a => a.Franchise);

            modelBuilder.Entity<Person>().HasMany(a => a.NotificationTokens).WithRequired(a => a.Person).HasForeignKey(x => x.PersonId);


            modelBuilder.Entity<PlaceReview>().HasOptional(a => a.SessionReview).WithOptionalDependent();

            modelBuilder.Entity<SessionReview>().HasOptional(a => a.PlaceReview).WithOptionalPrincipal();

            modelBuilder.Entity<PipeAccessoryReview>().HasMany(s => s.Medias).WithOptional(h => h.PipeAccessoryReview);

            modelBuilder.Entity<Place.Place>().HasMany(s => s.Medias).WithOptional(h => h.Place);

        }

        private SmokeSession CurrentSession(Hookah hookah)
        {
            var smokeSession = hookah.SmokeSessions.FirstOrDefault();

            if (smokeSession == null)
            {
                var newSession = this.SmokeSessions.Create();
                newSession.Hookah = hookah;
                return newSession;
            }

            return smokeSession;
        }
    }
}