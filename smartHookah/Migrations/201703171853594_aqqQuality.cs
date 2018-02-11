namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class aqqQuality : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "Quality", c => c.Double(nullable: false,defaultValueSql:"0"));
            AddColumn("dbo.PipeAccesoryStatistics", "SmokeTimePercentil", c => c.Double(nullable: false, defaultValueSql: "0"));
            AddColumn("dbo.PipeAccesoryStatistics", "SessionTimePercentil", c => c.Double(nullable: false, defaultValueSql: "0"));
            DropColumn("dbo.PipeAccesoryStatistics", "SmokeTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "SmokeTime", c => c.Double(nullable: false));
            DropColumn("dbo.PipeAccesoryStatistics", "SessionTimePercentil");
            DropColumn("dbo.PipeAccesoryStatistics", "SmokeTimePercentil");
            DropColumn("dbo.PipeAccesoryStatistics", "Quality");
        }
    }
}
