namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class review_refix : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id" });
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id1" });
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReviewId", newName: "SessionReview_Id");

            //RenameColumn(table: "dbo.SessionReview", name: "SessionReview_Id1", newName: "PlaceReview_Id1");
            //RenameColumn(table: "dbo.SessionReview", name: "SessionReview_Id", newName: "PlaceReview_Id");

            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReviewId", newName: "IX_SessionReview_Id");
            AddColumn("dbo.PlaceReview", "Service", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Ambience", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Overall", c => c.Int(nullable: false));
            //CreateIndex("dbo.SessionReview", "PlaceReview_Id");
            //CreateIndex("dbo.SessionReview", "PlaceReview_Id1");
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview");
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id1", "dbo.SessionReview");
            DropColumn("dbo.PlaceReview", "SessionReview_Id");
            DropColumn("dbo.PlaceReview", "SessionReview_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlaceReview", "SessionReview_Id1", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReview_Id", c => c.Int());
            DropIndex("dbo.SessionReview", new[] { "PlaceReview_Id1" });
            DropIndex("dbo.SessionReview", new[] { "PlaceReview_Id" });
            DropColumn("dbo.PlaceReview", "Overall");
            DropColumn("dbo.PlaceReview", "Ambience");
            DropColumn("dbo.PlaceReview", "Service");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReview_Id", newName: "IX_SessionReviewId");
            RenameColumn(table: "dbo.SessionReview", name: "PlaceReview_Id", newName: "SessionReview_Id");
            RenameColumn(table: "dbo.SessionReview", name: "PlaceReview_Id1", newName: "SessionReview_Id1");
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReview_Id", newName: "SessionReviewId");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id1");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id");
        }
    }
}
