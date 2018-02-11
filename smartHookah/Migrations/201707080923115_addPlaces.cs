namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPlaces : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Address",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Street = c.String(),
                        City = c.String(),
                        Number = c.String(),
                        ZIP = c.String(),
                        Lat = c.String(maxLength: 10),
                        Lng = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Place",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        LogoPath = c.String(),
                        ShortDescriptions = c.String(maxLength: 255),
                        Descriptions = c.String(),
                        FriendlyUrl = c.String(nullable: false, maxLength: 25),
                        AddressId = c.Int(nullable: false),
                        PersonId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Address", t => t.AddressId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.FriendlyUrl)
                .Index(t => t.AddressId)
                .Index(t => t.PersonId);
            
            CreateTable(
                "dbo.BusinessHours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LoungeId = c.Int(nullable: false),
                        PlaceId = c.Int(nullable: false),
                        Day = c.Int(nullable: false),
                        OpenTine = c.Time(nullable: false, precision: 7),
                        CloseTime = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .Index(t => t.PlaceId);
            
            AddColumn("dbo.Person", "PlaceId", c => c.Int());
            CreateIndex("dbo.Person", "PlaceId");
            AddForeignKey("dbo.Person", "PlaceId", "dbo.Place", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Person", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.Place", "PersonId", "dbo.Person");
            DropForeignKey("dbo.BusinessHours", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.Place", "AddressId", "dbo.Address");
            DropIndex("dbo.BusinessHours", new[] { "PlaceId" });
            DropIndex("dbo.Place", new[] { "PersonId" });
            DropIndex("dbo.Place", new[] { "AddressId" });
            DropIndex("dbo.Place", new[] { "FriendlyUrl" });
            DropIndex("dbo.Person", new[] { "PlaceId" });
            DropColumn("dbo.Person", "PlaceId");
            DropTable("dbo.BusinessHours");
            DropTable("dbo.Place");
            DropTable("dbo.Address");
        }
    }
}
