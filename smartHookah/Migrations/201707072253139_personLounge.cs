namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class personLounge : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Lounge", "AddressId", "dbo.Address");
            DropForeignKey("dbo.BusinessHours", "LoungeId", "dbo.Lounge");
            DropForeignKey("dbo.Person", "LoungeId", "dbo.Lounge");
            DropForeignKey("dbo.ApplicationUser", "LoungeId", "dbo.Lounge");
            DropIndex("dbo.Person", new[] { "LoungeId" });
            DropIndex("dbo.Lounge", new[] { "AddressId" });
            DropIndex("dbo.BusinessHours", new[] { "LoungeId" });
            DropIndex("dbo.ApplicationUser", new[] { "LoungeId" });
            DropColumn("dbo.Person", "LoungeId");
            DropColumn("dbo.ApplicationUser", "LoungeId");
            DropTable("dbo.Lounge");
            DropTable("dbo.Address");
            DropTable("dbo.BusinessHours");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BusinessHours",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LoungeId = c.Int(nullable: false),
                        Day = c.Int(nullable: false),
                        OpenTine = c.Time(nullable: false, precision: 7),
                        CloseTime = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Address",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Street = c.String(),
                        City = c.String(),
                        Number = c.String(),
                        ZIP = c.String(),
                        GoogleMapUri = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Lounge",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        LogoPath = c.String(),
                        ShortDescriptions = c.String(maxLength: 255),
                        Descriptions = c.String(),
                        FriendlyUrl = c.String(nullable: false, maxLength: 25),
                        AddressId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ApplicationUser", "LoungeId", c => c.Int());
            AddColumn("dbo.Person", "LoungeId", c => c.Int());
            CreateIndex("dbo.ApplicationUser", "LoungeId");
            CreateIndex("dbo.BusinessHours", "LoungeId");
            CreateIndex("dbo.Lounge", "AddressId");
            CreateIndex("dbo.Person", "LoungeId");
            AddForeignKey("dbo.ApplicationUser", "LoungeId", "dbo.Lounge", "Id");
            AddForeignKey("dbo.Person", "LoungeId", "dbo.Lounge", "Id");
            AddForeignKey("dbo.BusinessHours", "LoungeId", "dbo.Lounge", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Lounge", "AddressId", "dbo.Address", "Id", cascadeDelete: true);
        }
    }
}
