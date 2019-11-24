namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class reviewUpd4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlaceReview", "SessionReview_Id", c => c.Int());
            CreateIndex("dbo.PlaceReview", "SessionReview_Id");
            AddForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.PlaceReview", "SessionReview_Id", "dbo.SessionReview");
            DropIndex("dbo.PlaceReview", new[] { "SessionReview_Id" });
            DropColumn("dbo.PlaceReview", "SessionReview_Id");
        }
    }
}
