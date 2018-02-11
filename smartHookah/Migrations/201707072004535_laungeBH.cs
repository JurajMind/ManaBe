namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class laungeBH : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lounge", t => t.LoungeId, cascadeDelete: true)
                .Index(t => t.LoungeId);
            
            AddColumn("dbo.Lounge", "ShortDescriptions", c => c.String(maxLength: 255));
            AddColumn("dbo.Lounge", "FriendlyUrl", c => c.String(nullable: false, maxLength: 25));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BusinessHours", "LoungeId", "dbo.Lounge");
            DropIndex("dbo.BusinessHours", new[] { "LoungeId" });
            DropColumn("dbo.Lounge", "FriendlyUrl");
            DropColumn("dbo.Lounge", "ShortDescriptions");
            DropTable("dbo.BusinessHours");
        }
    }
}
