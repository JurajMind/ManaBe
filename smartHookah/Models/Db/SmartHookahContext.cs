using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.AspNet.Identity.EntityFramework;
using smartHookah.Models.Db;

namespace smartHookah.Models
{
    public class SmartHookahContext : IdentityDbContext<ApplicationUser>
    {
        public SmartHookahContext() : base("SmartHookah")
        {
#if DEBUG
            this.Database.Log = Console.WriteLine;
#endif
        }

        public static SmartHookahContext Create()
        {
            return new SmartHookahContext();
        }

        public DbSet<Hookah> Hookahs { get; set; }

        public DbSet<Place> Places { get; set; }

        public DbSet<BusinessHours> BusinessHours { get; set; }

        public DbSet<SmokeSession> SmokeSessions { get; set; }
        
        public DbSet<DbPuf> DbPufs { get; set; }

        public DbSet<Tobacco> Tobaccos { get; set; }

        public DbSet<TobaccoMix> TobaccoMixs { get; set; }

        public DbSet<TobacoMixPart> TobaccosMixParts { get; set; }

        public DbSet<Bowl>Bowls { get; set; }

        public DbSet<Pipe> Pipes { get; set; }

        public DbSet<PipeAccesory> PipeAccesories { get; set; }

        public DbSet<OwnPipeAccesories> OwnPipeAccesorieses { get; set; }

        public DbSet<Person> Persons { get; set; }

        public DbSet<Brand> Brands { get; set; }
        
        public DbSet<SmokeSessionStatistics> SessionStatistics { get; set; }

        public DbSet<SmokeSessionMetaData> SessionMetaDatas { get; set; }

        public DbSet<PipeAccesoryStatistics> PipeAccesoryStatistics { get; set; }
        
        public DbSet<HookahSetting> HookahSettings { get; set; }

        public DbSet<Update> Updates { get; set; }

        public DbSet<UpdateLog> UpdateLogs { get; set; }
       
        public DbSet<HookahOrder> HookahOrders { get; set; }

        public DbSet<OrderExtra> OrderExtras { get; set; }
        
        public DbSet<Event> Events { get; set; }

        public DbSet<GameProfile> GameProfiles { get; set; }

        public DbSet<Reward> Rewards { get; set; }

        public DbSet<EventProgress> EventProgresses { get; set; }

        public DbSet<Seat> Seats { get; set; }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<TobaccoTaste> TobaccoTastes { get; set; }

        public DbSet<PriceGroupPrice> PriceGroupPrice { get; set; }

        public DbSet<PriceGroup> PriceGroup { get; set; }

        public DbSet<PlaceDay> PlaceDays { get; set; }

        public DbSet<PlaceEvent> PlaceEvents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<IdentityUserRole>()
            .HasKey(r => new { r.UserId, r.RoleId })
            .ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserLogin>()
              .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
            .ToTable("AspNetUserLogins");

            modelBuilder.Entity<SmokeSession>().HasOptional(a => a.Statistics);

            modelBuilder.Entity<Person>().HasMany<SmokeSession>(s => s.SmokeSessions).WithMany(c => c.Persons).Map(
                cs =>
                {
                    cs.MapLeftKey("SmokeSessionRefId");
                    cs.MapRightKey("PersonRefId");
                    cs.ToTable("SmokeSessionPerson");
                });

            modelBuilder.Entity<Person>().HasMany<Hookah>(s => s.Hookahs).WithMany(h => h.Owners).Map(
                cs =>
                {
                    cs.MapLeftKey("HookahRefId");
                    cs.MapRightKey("PersonRefId");
                    cs.ToTable("HookahOwning");
                });

            modelBuilder.Entity<SmokeSessionMetaData>().HasMany<Tag>(s => s.Tags).WithMany(h => h.Used).Map(
                cs =>
                {
                    cs.MapLeftKey("SmokeSessionMetadataRefId");
                    cs.MapRightKey("TagRefId");
                    cs.ToTable("SmokeSessionMetadataTags");
                });



            modelBuilder.Entity<Person>().HasMany<Place>(s => s.Manage).WithMany(h => h.Managers).Map(
                cs =>
                {
                    cs.MapLeftKey("PersonRefId");
                    cs.MapRightKey("PlaceRefId");
                    cs.ToTable("PlaceManagers");
                });

            modelBuilder.Entity<Tobacco>().HasMany<TobaccoTaste>(s => s.Tastes).WithMany(h => h.Tobaccos).Map(
                cs =>
                {
                    cs.MapLeftKey("TobaccoRefId");
                    cs.MapRightKey("TasteRefId");
                    cs.ToTable("TobaccoTasteBinding");
                });

            modelBuilder.Entity<Person>().HasOptional(a => a.GameProfile).WithRequired(a => a.Person);

            modelBuilder.Entity<Person>()
                .HasMany<OwnPipeAccesories>(s => s.OwnedPipeAccesories)
                .WithRequired(a => a.Person);

            modelBuilder.Entity<Brand>().HasMany<PipeAccesory>(s => s.PipeAccesories).WithRequired(a => a.Brand);

            modelBuilder.Entity<SmokeSession>().HasOptional(a => a.Review).WithOptionalDependent();

            modelBuilder.Entity<PipeAccesory>().HasOptional(a => a.Statistics).WithRequired(a => a.PipeAccesory);

            modelBuilder.Entity<Place>().HasOptional(a => a.Person).WithMany().HasForeignKey(a => a.PersonId);

            modelBuilder.Entity<Person>().HasOptional(a => a.AssignedBrand);

            modelBuilder.Entity<SmokeSession>().HasMany(a => a.SmokeEvents).WithRequired(a => a.SmokeSession);

            modelBuilder.Entity<Place>().HasMany(a => a.SmokeSessions).WithOptional(a => a.Place);

            modelBuilder.Entity<Person>().HasMany(a => a.Places).WithOptional(a => a.Person);

            modelBuilder.Entity<Person>().HasOptional(a => a.Game).WithRequired(a => a.Person);

            modelBuilder.Entity<Reservation>().HasMany<Person>(s => s.Customers).WithMany(h => h.InReservations).Map(
                cs =>
                {
                    cs.MapLeftKey("ReservationRefId");
                    cs.MapRightKey("PersonRefId");
                    cs.ToTable("ReservationCustomers");
                });

            modelBuilder.Entity<Reservation>().HasMany<Person>(s => s.Customers).WithMany(h => h.InReservations).Map(
                cs =>
                {
                    cs.MapLeftKey("ReservationRefId");
                    cs.MapRightKey("PersonRefId");
                    cs.ToTable("ReservationCustomers");
                });

            modelBuilder.Entity<Reservation>().HasMany<Seat>(s => s.Seats).WithMany(h => h.Reservations).Map(
                cs =>
                {
                    cs.MapLeftKey("ReservationRefId");
                    cs.MapRightKey("SeatRefId");
                    cs.ToTable("ReservationSeat");
                });

            modelBuilder.Entity<OwnPipeAccesories>().HasMany<PriceGroupPrice>(s => s.Prices)
                .WithRequired(a => a.PipeAccesorie);

            modelBuilder.Entity<PipeAccesory>().HasMany<SimilarAccesories>(s => s.SimilarAccesories).WithRequired(h => h.Original).WillCascadeOnDelete(false);

            modelBuilder.Entity<Place>().HasMany(p => p.PlaceDays);

            modelBuilder.Entity<PlaceDay>().HasMany(p => p.PlaceEvents);

            modelBuilder.Entity<PlaceEvent>().HasMany(p => p.Persons).WithMany(e => e.PlaceEvents);

        }

        public SmokeSession GetCurrentSession(int id)
        {
            var hookah = this.Hookahs.Find(id);
            return CurrentSession(hookah);
        }

        public SmokeSession GetSmokeSession(string hookahCode)
        {
            var hookah = this.Hookahs.FirstOrDefault(h => h.Code == hookahCode);
            return CurrentSession(hookah);
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

        public System.Data.Entity.DbSet<smartHookah.Models.StandPicture> StandPictures { get; set; }

        public System.Data.Entity.DbSet<smartHookah.Models.TobaccoReview> TobaccoReviews { get; set; }

       public System.Data.Entity.DbSet<smartHookah.Models.Address> Addresses { get; set; }

        public System.Data.Entity.DbSet<smartHookah.Models.EventRecepie> EventRecepies { get; set; }

        public System.Data.Entity.DbSet<smartHookah.Models.Media> Media { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Client> Clients { get; set; }
    }
}