namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DevicePresetRefuckup : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Person", "DefaultPresetId");
            RenameColumn(table: "dbo.Person", name: "DefaultPreset_DevicePresetId", newName: "DefaultPresetId");
            RenameColumn(table: "dbo.DevicePreset", name: "DevicePresetId", newName: "Id");
            RenameIndex(table: "dbo.Person", name: "IX_DefaultPreset_DevicePresetId", newName: "IX_DefaultPresetId");
            RenameIndex(table: "dbo.DevicePreset", name: "IX_DevicePresetId", newName: "IX_Id");
        }

        public override void Down()
        {
            RenameIndex(table: "dbo.DevicePreset", name: "IX_Id", newName: "IX_DevicePresetId");
            RenameIndex(table: "dbo.Person", name: "IX_DefaultPresetId", newName: "IX_DefaultPreset_DevicePresetId");
            RenameColumn(table: "dbo.DevicePreset", name: "Id", newName: "DevicePresetId");
            RenameColumn(table: "dbo.Person", name: "DefaultPresetId", newName: "DefaultPreset_DevicePresetId");
            AddColumn("dbo.Person", "DefaultPresetId", c => c.Int());
        }
    }
}
