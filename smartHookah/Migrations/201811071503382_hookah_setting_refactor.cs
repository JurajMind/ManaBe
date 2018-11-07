namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookah_setting_refactor : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.HookahSetting", newName: "DeviceSetting");
            DropForeignKey("dbo.DevicePreset", "SettingId", "dbo.HookahSetting");
            DropIndex("dbo.DevicePreset", new[] { "SettingId" });
            DropColumn("dbo.DevicePreset", "Id");
            RenameColumn(table: "dbo.DevicePreset", name: "SettingId", newName: "Id");
            DropPrimaryKey("dbo.DevicePreset");
            AddColumn("dbo.DeviceSetting", "DevicePresetId", c => c.Int());
            AddColumn("dbo.DevicePreset", "DeviceSettingId", c => c.Int(nullable: false));
            AlterColumn("dbo.DevicePreset", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.DevicePreset", "Id");
            CreateIndex("dbo.DevicePreset", "Id");
            AddForeignKey("dbo.DevicePreset", " Id", "dbo.DeviceSetting", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DevicePreset", "Id", "dbo.DeviceSetting");
            DropIndex("dbo.DevicePreset", new[] { "Id" });
            DropPrimaryKey("dbo.DevicePreset");
            AlterColumn("dbo.DevicePreset", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.DevicePreset", "DeviceSettingId");
            DropColumn("dbo.DeviceSetting", "DevicePresetId");
            AddPrimaryKey("dbo.DevicePreset", "Id");
            RenameColumn(table: "dbo.DevicePreset", name: "Id", newName: "SettingId");
            AddColumn("dbo.DevicePreset", "Id", c => c.Int(nullable: false, identity: true));
            CreateIndex("dbo.DevicePreset", "SettingId");
            AddForeignKey("dbo.DevicePreset", "SettingId", "dbo.HookahSetting", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.DeviceSetting", newName: "HookahSetting");
        }
    }
}
