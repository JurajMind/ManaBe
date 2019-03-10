namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placeFlag : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlaceFlag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        DisplayName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PlaceFlagMapping",
                c => new
                    {
                        PlaceRefId = c.Int(nullable: false),
                        FlagRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlaceRefId, t.FlagRefId })
                .ForeignKey("dbo.Place", t => t.PlaceRefId, cascadeDelete: true)
                .ForeignKey("dbo.PlaceFlag", t => t.FlagRefId, cascadeDelete: true)
                .Index(t => t.PlaceRefId)
                .Index(t => t.FlagRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlaceFlagMapping", "FlagRefId", "dbo.PlaceFlag");
            DropForeignKey("dbo.PlaceFlagMapping", "PlaceRefId", "dbo.Place");
            DropIndex("dbo.PlaceFlagMapping", new[] { "FlagRefId" });
            DropIndex("dbo.PlaceFlagMapping", new[] { "PlaceRefId" });
            DropTable("dbo.PlaceFlagMapping");
            DropTable("dbo.PlaceFlag");
        }
    }
}
