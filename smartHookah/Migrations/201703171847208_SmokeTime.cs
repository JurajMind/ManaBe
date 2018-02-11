namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SmokeTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "SmokeTime", c => c.Double(nullable: false,defaultValueSql:"0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PipeAccesoryStatistics", "SmokeTime");
        }
    }
}
