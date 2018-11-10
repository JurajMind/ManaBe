namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookah_setting_refactor_id : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DevicePreset", name: "SettingId", newName: "Id");
            RenameIndex(table: "dbo.DevicePreset", name: "IX_SettingId", newName: "IX_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.DevicePreset", name: "IX_Id", newName: "IX_SettingId");
            RenameColumn(table: "dbo.DevicePreset", name: "Id", newName: "SettingId");
        }
    }
}
