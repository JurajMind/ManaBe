namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Coal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSessionMetaData", "CoalType", c => c.Int(nullable: false,defaultValueSql:"0"));
            AddColumn("dbo.SmokeSessionMetaData", "CoalsCount", c => c.Double(nullable: false,defaultValueSql:"2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmokeSessionMetaData", "CoalsCount");
            DropColumn("dbo.SmokeSessionMetaData", "CoalType");
        }
    }
}
