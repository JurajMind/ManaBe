namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reservation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reservation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        PersonId = c.Int(),
                        PlaceId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Persons = c.Int(nullable: false),
                        Duration = c.Time(nullable: false, precision: 7),
                        Time = c.DateTime(nullable: false),
                        SeatId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .ForeignKey("dbo.Seat", t => t.SeatId)
                .Index(t => t.PersonId)
                .Index(t => t.PlaceId)
                .Index(t => t.SeatId);
            
            AddColumn("dbo.Person", "PersonRating", c => c.Int(nullable: false));
            AddColumn("dbo.HookahOrder", "Created", c => c.DateTime(nullable: false));
            AddColumn("dbo.HookahOrder", "ReservationId", c => c.Int());
            AddColumn("dbo.Seat", "Capacity", c => c.Int(nullable: false));
            CreateIndex("dbo.HookahOrder", "ReservationId");
            AddForeignKey("dbo.HookahOrder", "ReservationId", "dbo.Reservation", "Id");
            DropColumn("dbo.HookahOrder", "CreateDateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.HookahOrder", "CreateDateTime", c => c.DateTime(nullable: false));
            DropForeignKey("dbo.Reservation", "SeatId", "dbo.Seat");
            DropForeignKey("dbo.Reservation", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.Reservation", "PersonId", "dbo.Person");
            DropForeignKey("dbo.HookahOrder", "ReservationId", "dbo.Reservation");
            DropIndex("dbo.Reservation", new[] { "SeatId" });
            DropIndex("dbo.Reservation", new[] { "PlaceId" });
            DropIndex("dbo.Reservation", new[] { "PersonId" });
            DropIndex("dbo.HookahOrder", new[] { "ReservationId" });
            DropColumn("dbo.Seat", "Capacity");
            DropColumn("dbo.HookahOrder", "ReservationId");
            DropColumn("dbo.HookahOrder", "Created");
            DropColumn("dbo.Person", "PersonRating");
            DropTable("dbo.Reservation");
        }
    }
}
