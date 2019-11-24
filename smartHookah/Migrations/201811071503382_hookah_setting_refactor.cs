namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class hookah_setting_refactor : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.HookahSetting", newName: "DeviceSetting");
            DropForeignKey("dbo.DevicePreset", "SettingId", "dbo.HookahSetting");
            DropPrimaryKey("dbo.DevicePreset");
            AddColumn("dbo.DeviceSetting", "DevicePresetId", c => c.Int());
            AddColumn("dbo.DevicePreset", "DeviceSettingId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.DevicePreset", "SettingId");
            AddForeignKey("dbo.DevicePreset", "SettingId", "dbo.DeviceSetting", "Id");
            DropColumn("dbo.DevicePreset", "Id");
        }

        public override void Down()
        {
            AddColumn("dbo.DevicePreset", "Id", c => c.Int(nullable: false, identity: true));
            DropForeignKey("dbo.DevicePreset", "SettingId", "dbo.DeviceSetting");
            DropPrimaryKey("dbo.DevicePreset");
            DropColumn("dbo.DevicePreset", "DeviceSettingId");
            DropColumn("dbo.DeviceSetting", "DevicePresetId");
            AddPrimaryKey("dbo.DevicePreset", "Id");
            AddForeignKey("dbo.DevicePreset", "SettingId", "dbo.HookahSetting", "Id", cascadeDelete: true);
            RenameTable(name: "dbo.DeviceSetting", newName: "HookahSetting");
        }
    }
}
