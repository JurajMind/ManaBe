namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewUpd2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccessoryReview", "Cut", c => c.Int());
            AddColumn("dbo.PipeAccessoryReview", "Strength", c => c.Int());
            AddColumn("dbo.PipeAccessoryReview", "Duration", c => c.Int());
            AddColumn("dbo.PipeAccesoryStatistics", "Cut", c => c.Double(nullable: false));
            AddColumn("dbo.PipeAccesoryStatistics", "Strength", c => c.Double(nullable: false));
            AddColumn("dbo.PipeAccesoryStatistics", "Duration", c => c.Double(nullable: false));
            AddColumn("dbo.SessionReview", "nsTaste", c => c.Int());
            AddColumn("dbo.SessionReview", "nsStrength", c => c.Int());
            AddColumn("dbo.SessionReview", "nsDuration", c => c.Int());
            AddColumn("dbo.SessionReview", "nsSmoke", c => c.Int());
            DropColumn("dbo.PipeAccessoryReview", "Quality");
            DropColumn("dbo.PipeAccesoryStatistics", "Quality");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "Quality", c => c.Double(nullable: false));
            AddColumn("dbo.PipeAccessoryReview", "Quality", c => c.Int());
            DropColumn("dbo.SessionReview", "nsSmoke");
            DropColumn("dbo.SessionReview", "nsDuration");
            DropColumn("dbo.SessionReview", "nsStrength");
            DropColumn("dbo.SessionReview", "nsTaste");
            DropColumn("dbo.PipeAccesoryStatistics", "Duration");
            DropColumn("dbo.PipeAccesoryStatistics", "Strength");
            DropColumn("dbo.PipeAccesoryStatistics", "Cut");
            DropColumn("dbo.PipeAccessoryReview", "Duration");
            DropColumn("dbo.PipeAccessoryReview", "Strength");
            DropColumn("dbo.PipeAccessoryReview", "Cut");
        }
    }
}
