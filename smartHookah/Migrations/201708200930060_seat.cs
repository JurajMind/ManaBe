namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Seat",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 64),
                        PlaceId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId)
                .Index(t => t.PlaceId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Seat", "PlaceId", "dbo.Place");
            DropIndex("dbo.Seat", new[] { "PlaceId" });
            DropTable("dbo.Seat");
        }
    }
}
