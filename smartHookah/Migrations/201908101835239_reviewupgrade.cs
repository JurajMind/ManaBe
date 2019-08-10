namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewupgrade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PipeAccessoryReview", "SmokeSessionId", "dbo.SmokeSession");
            DropIndex("dbo.PipeAccessoryReview", new[] { "SmokeSessionId" });
            CreateTable(
                "dbo.SessionReview",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorId = c.Int(),
                        PublishDate = c.DateTime(nullable: false),
                        SmokeSessionId = c.Int(nullable: false),
                        TobaccoReview_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorId)
                .ForeignKey("dbo.SmokeSession", t => t.SmokeSessionId, cascadeDelete: true)
                .ForeignKey("dbo.PipeAccessoryReview", t => t.TobaccoReview_Id)
                .Index(t => t.AuthorId)
                .Index(t => t.SmokeSessionId)
                .Index(t => t.TobaccoReview_Id);
            
            AddColumn("dbo.Media", "SessionReview_Id", c => c.Int());
            AddColumn("dbo.Media", "PlaceReview_Id", c => c.Int());
            AddColumn("dbo.Media", "PipeAccessoryReview_Id", c => c.Int());
            AddColumn("dbo.PipeAccessoryReview", "SessionReviewId", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReviewId", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReview_Id", c => c.Int());
            AddColumn("dbo.PlaceReview", "SessionReview_Id1", c => c.Int());
            CreateIndex("dbo.Media", "SessionReview_Id");
            CreateIndex("dbo.Media", "PlaceReview_Id");
            CreateIndex("dbo.Media", "PipeAccessoryReview_Id");
            CreateIndex("dbo.PipeAccessoryReview", "SessionReviewId");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id");
            CreateIndex("dbo.PlaceReview", "SessionReview_Id1");
            AddForeignKey("dbo.PipeAccessoryReview", "SessionReviewId", "dbo.SessionReview", "Id");
            AddForeignKey("dbo.Media", "SessionReview_Id", "dbo.SessionReview", "Id");
            AddForeignKey("dbo.Media", "PlaceReview_Id", "dbo.PlaceReview", "Id");
            AddForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview", "Id");
            AddForeignKey("dbo.PlaceReview", "SessionReview_Id1", "dbo.SessionReview", "Id");
            AddForeignKey("dbo.Media", "PipeAccessoryReview_Id", "dbo.PipeAccessoryReview", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Media", "PipeAccessoryReview_Id", "dbo.PipeAccessoryReview");
            DropForeignKey("dbo.SessionReview", "TobaccoReview_Id", "dbo.PipeAccessoryReview");
            DropForeignKey("dbo.SessionReview", "SmokeSessionId", "dbo.SmokeSession");
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id1", "dbo.SessionReview");
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview");
            DropForeignKey("dbo.Media", "PlaceReview_Id", "dbo.PlaceReview");
            DropForeignKey("dbo.Media", "SessionReview_Id", "dbo.SessionReview");
            DropForeignKey("dbo.PipeAccessoryReview", "SessionReviewId", "dbo.SessionReview");
            DropForeignKey("dbo.SessionReview", "AuthorId", "dbo.Person");
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id1" });
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id" });
            DropIndex("dbo.SessionReview", new[] { "TobaccoReview_Id" });
            DropIndex("dbo.SessionReview", new[] { "SmokeSessionId" });
            DropIndex("dbo.SessionReview", new[] { "AuthorId" });
            DropIndex("dbo.PipeAccessoryReview", new[] { "SessionReviewId" });
            DropIndex("dbo.Media", new[] { "PipeAccessoryReview_Id" });
            DropIndex("dbo.Media", new[] { "PlaceReview_Id" });
            DropIndex("dbo.Media", new[] { "SessionReview_Id" });
            DropColumn("dbo.PlaceReview", "SessionReview_Id1");
            DropColumn("dbo.PlaceReview", "SessionReview_Id");
            DropColumn("dbo.PlaceReview", "SessionReviewId");
            DropColumn("dbo.PipeAccessoryReview", "SessionReviewId");
            DropColumn("dbo.Media", "PipeAccessoryReview_Id");
            DropColumn("dbo.Media", "PlaceReview_Id");
            DropColumn("dbo.Media", "SessionReview_Id");
            DropTable("dbo.SessionReview");
            CreateIndex("dbo.PipeAccessoryReview", "SmokeSessionId");
            AddForeignKey("dbo.PipeAccessoryReview", "SmokeSessionId", "dbo.SmokeSession", "Id");
        }
    }
}
