namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game6 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlaceManagers",
                c => new
                    {
                        PersonRefId = c.Int(nullable: false),
                        PlaceRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PersonRefId, t.PlaceRefId })
                .ForeignKey("dbo.Person", t => t.PersonRefId, cascadeDelete: true)
                .ForeignKey("dbo.Place", t => t.PlaceRefId, cascadeDelete: true)
                .Index(t => t.PersonRefId)
                .Index(t => t.PlaceRefId);
            
            AddColumn("dbo.SmokeSession", "PlaceId", c => c.Int());
            CreateIndex("dbo.SmokeSession", "PlaceId");
            AddForeignKey("dbo.SmokeSession", "PlaceId", "dbo.Place", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlaceManagers", "PlaceRefId", "dbo.Place");
            DropForeignKey("dbo.PlaceManagers", "PersonRefId", "dbo.Person");
            DropForeignKey("dbo.SmokeSession", "PlaceId", "dbo.Place");
            DropIndex("dbo.PlaceManagers", new[] { "PlaceRefId" });
            DropIndex("dbo.PlaceManagers", new[] { "PersonRefId" });
            DropIndex("dbo.SmokeSession", new[] { "PlaceId" });
            DropColumn("dbo.SmokeSession", "PlaceId");
            DropTable("dbo.PlaceManagers");
        }
    }
}
