namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlaceCalendar : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlaceDay",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Day = c.DateTime(nullable: false),
                        OpenHour = c.Time(nullable: false, precision: 7),
                        CloseHour = c.Time(nullable: false, precision: 7),
                        PlaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .Index(t => t.PlaceId);
            
            CreateTable(
                "dbo.PlaceEvent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        PrivacyType = c.String(),
                        FacebookUrl = c.String(),
                        PlaceDayId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PlaceDay", t => t.PlaceDayId, cascadeDelete: true)
                .Index(t => t.PlaceDayId);
            
            CreateTable(
                "dbo.PlaceEventPerson",
                c => new
                    {
                        PlaceEvent_Id = c.Int(nullable: false),
                        Person_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlaceEvent_Id, t.Person_Id })
                .ForeignKey("dbo.PlaceEvent", t => t.PlaceEvent_Id, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.Person_Id, cascadeDelete: true)
                .Index(t => t.PlaceEvent_Id)
                .Index(t => t.Person_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlaceDay", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.PlaceEvent", "PlaceDayId", "dbo.PlaceDay");
            DropForeignKey("dbo.PlaceEventPerson", "Person_Id", "dbo.Person");
            DropForeignKey("dbo.PlaceEventPerson", "PlaceEvent_Id", "dbo.PlaceEvent");
            DropIndex("dbo.PlaceEventPerson", new[] { "Person_Id" });
            DropIndex("dbo.PlaceEventPerson", new[] { "PlaceEvent_Id" });
            DropIndex("dbo.PlaceEvent", new[] { "PlaceDayId" });
            DropIndex("dbo.PlaceDay", new[] { "PlaceId" });
            DropTable("dbo.PlaceEventPerson");
            DropTable("dbo.PlaceEvent");
            DropTable("dbo.PlaceDay");
        }
    }
}
