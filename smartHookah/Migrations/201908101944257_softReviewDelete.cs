namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class softReviewDelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccessoryReview", "Deleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.SessionReview", "Deleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.PlaceReview", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlaceReview", "Deleted");
            DropColumn("dbo.SessionReview", "Deleted");
            DropColumn("dbo.PipeAccessoryReview", "Deleted");
        }
    }
}
