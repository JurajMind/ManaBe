namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Device_preset_refactor : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.HookahPersonSetting", newName: "DevicePreset");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.DevicePreset", newName: "HookahPersonSetting");
        }
    }
}
