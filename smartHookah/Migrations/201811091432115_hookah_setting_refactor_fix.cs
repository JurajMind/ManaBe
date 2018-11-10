namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookah_setting_refactor_fix : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DevicePreset", "DeviceSettingId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DevicePreset", "DeviceSettingId", c => c.Int(nullable: false));
        }
    }
}
