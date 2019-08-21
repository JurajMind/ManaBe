namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewIdFix : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReviewId", newName: "SessionReview_Id");
            RenameColumn(table: "dbo.SessionReview", name: "TobaccoReview_Id", newName: "TobaccoReviewId");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReviewId", newName: "IX_SessionReview_Id");
            RenameIndex(table: "dbo.SessionReview", name: "IX_TobaccoReview_Id", newName: "IX_TobaccoReviewId");
            AddColumn("dbo.SessionReview", "PlaceReviewId", c => c.Int());
            DropColumn("dbo.PlaceReview", "SessionReviewId");
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id1" });
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id" });
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlaceReview", "SessionReviewId", c => c.Int());
            DropColumn("dbo.SessionReview", "PlaceReviewId");
            RenameIndex(table: "dbo.SessionReview", name: "IX_TobaccoReviewId", newName: "IX_TobaccoReview_Id");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReview_Id", newName: "IX_SessionReviewId");
            RenameColumn(table: "dbo.SessionReview", name: "TobaccoReviewId", newName: "TobaccoReview_Id");
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReview_Id", newName: "SessionReviewId");
        }
    }
}
