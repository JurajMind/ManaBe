namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class brightness : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahSetting", "IdleBrightness", c => c.Int(nullable: false,defaultValueSql:"20"));
            AddColumn("dbo.HookahSetting", "PufBrightness", c => c.Int(nullable: false, defaultValueSql: "255"));
            AddColumn("dbo.HookahSetting", "Bt", c => c.Int(nullable: false, defaultValueSql: "0"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HookahSetting", "Bt");
            DropColumn("dbo.HookahSetting", "PufBrightness");
            DropColumn("dbo.HookahSetting", "IdleBrightness");
        }
    }
}
