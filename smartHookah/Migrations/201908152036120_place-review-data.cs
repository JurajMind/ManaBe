namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placereviewdata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PlaceReview", "Service", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Ambience", c => c.Int(nullable: false));
            AddColumn("dbo.PlaceReview", "Overall", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PlaceReview", "Overall");
            DropColumn("dbo.PlaceReview", "Ambience");
            DropColumn("dbo.PlaceReview", "Service");
        }
    }
}
