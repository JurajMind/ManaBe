namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SessionReport : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "Report", c => c.Int(nullable: false,defaultValueSql:"0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmokeSession", "Report");
        }
    }
}
