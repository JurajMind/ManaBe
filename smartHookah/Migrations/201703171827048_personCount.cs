namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class personCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "Taste", c => c.Double(nullable: false,defaultValueSql:"0"));
            AddColumn("dbo.PipeAccesoryStatistics", "Smoke", c => c.Double(nullable: false, defaultValueSql: "0"));
            AddColumn("dbo.PipeAccesoryStatistics", "Overall", c => c.Double(nullable: false, defaultValueSql: "0"));
            AddColumn("dbo.SmokeSessionMetaData", "AnonymPeopleCount", c => c.Int(nullable: false, defaultValueSql: "0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmokeSessionMetaData", "AnonymPeopleCount");
            DropColumn("dbo.PipeAccesoryStatistics", "Overall");
            DropColumn("dbo.PipeAccesoryStatistics", "Smoke");
            DropColumn("dbo.PipeAccesoryStatistics", "Taste");
        }
    }
}
