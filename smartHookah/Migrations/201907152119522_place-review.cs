namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placereview : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.TobaccoReview", newName: "PipeAccessoryReview");
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "ReviewedTobaccoId", newName: "AccessorId");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_ReviewedTobaccoId", newName: "IX_AccessorId");
            CreateTable(
                "dbo.PlaceReview",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorId = c.Int(),
                        PublishDate = c.DateTime(nullable: false),
                        Text = c.String(),
                        PlaceId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorId)
                .ForeignKey("dbo.Place", t => t.PlaceId)
                .Index(t => t.AuthorId)
                .Index(t => t.PlaceId);
            
            AddColumn("dbo.PipeAccesory", "Rating", c => c.Double(nullable: false));
            AddColumn("dbo.PipeAccessoryReview", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.PipeAccessoryReview", "Quality", c => c.Int());
            AlterColumn("dbo.PipeAccessoryReview", "Taste", c => c.Int());
            AlterColumn("dbo.PipeAccessoryReview", "Smoke", c => c.Int());
            AlterColumn("dbo.PipeAccessoryReview", "Overall", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlaceReview", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.PlaceReview", "AuthorId", "dbo.Person");
            DropIndex("dbo.PlaceReview", new[] { "PlaceId" });
            DropIndex("dbo.PlaceReview", new[] { "AuthorId" });
            AlterColumn("dbo.PipeAccessoryReview", "Overall", c => c.Int(nullable: false));
            AlterColumn("dbo.PipeAccessoryReview", "Smoke", c => c.Int(nullable: false));
            AlterColumn("dbo.PipeAccessoryReview", "Taste", c => c.Int(nullable: false));
            AlterColumn("dbo.PipeAccessoryReview", "Quality", c => c.Int(nullable: false));
            DropColumn("dbo.PipeAccessoryReview", "Discriminator");
            DropColumn("dbo.PipeAccesory", "Rating");
            DropTable("dbo.PlaceReview");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_AccessorId", newName: "IX_ReviewedTobaccoId");
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "AccessorId", newName: "ReviewedTobaccoId");
            RenameTable(name: "dbo.PipeAccessoryReview", newName: "TobaccoReview");
        }
    }
}
