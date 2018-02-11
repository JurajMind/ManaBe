namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class review5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TobaccoReview", "Quality", c => c.Int(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Taste", c => c.Int(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Smoke", c => c.Int(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Overall", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TobaccoReview", "Overall", c => c.Double(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Smoke", c => c.Double(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Taste", c => c.Double(nullable: false));
            AlterColumn("dbo.TobaccoReview", "Quality", c => c.Double(nullable: false));
        }
    }
}
