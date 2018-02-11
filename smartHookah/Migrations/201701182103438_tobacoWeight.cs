namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tobacoWeight : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSessionMetaData", "TobaccoWeight", c => c.Double(nullable: false));
            AlterColumn("dbo.PipeAccesoryStatistics", "Weight", c => c.Double(nullable: false));
            AlterColumn("dbo.PipeAccesoryStatistics", "PufCount", c => c.Double(nullable: false));
            AlterColumn("dbo.PipeAccesoryStatistics", "BlowCount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PipeAccesoryStatistics", "BlowCount", c => c.Int(nullable: false));
            AlterColumn("dbo.PipeAccesoryStatistics", "PufCount", c => c.Int(nullable: false));
            AlterColumn("dbo.PipeAccesoryStatistics", "Weight", c => c.Int(nullable: false));
            DropColumn("dbo.SmokeSessionMetaData", "TobaccoWeight");
        }
    }
}
