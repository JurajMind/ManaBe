namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewUpd3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview");
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id1", "dbo.SessionReview");
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id" });
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id1" });
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReviewId", newName: "SessionReview_Id");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReviewId", newName: "IX_SessionReview_Id");
            AddColumn("dbo.PlaceReview", "Service", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Ambience", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Overall", c => c.Int(nullable: false));
            DropColumn("dbo.PlaceReview", "SessionReviewId");
            DropColumn("dbo.PlaceReview", "SessionReview_Id");
            DropColumn("dbo.PlaceReview", "SessionReview_Id1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlaceReview", "SessionReview_Id1", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReview_Id", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReviewId", c => c.Int());
            DropColumn("dbo.PlaceReview", "Overall");
            DropColumn("dbo.PlaceReview", "Ambience");
            DropColumn("dbo.PlaceReview", "Service");
            RenameIndex(table: "dbo.PipeAccessoryReview", name: "IX_SessionReview_Id", newName: "IX_SessionReviewId");
            RenameColumn(table: "dbo.PipeAccessoryReview", name: "SessionReview_Id", newName: "SessionReviewId");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id1");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id");
            AddForeignKey("dbo.PlaceReview", "SessionReview_Id1", "dbo.SessionReview", "Id");
            AddForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview", "Id");
        }
    }
}
