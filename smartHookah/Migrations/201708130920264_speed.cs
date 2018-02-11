namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class speed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahSetting", "IdleSpeed", c => c.Int(nullable: false,defaultValueSql:"70"));
            AddColumn("dbo.HookahSetting", "PufSpeed", c => c.Int(nullable: false, defaultValueSql: "100"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HookahSetting", "PufSpeed");
            DropColumn("dbo.HookahSetting", "IdleSpeed");
        }
    }
}
