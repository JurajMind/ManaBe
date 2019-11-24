namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class DevicePresetFuckup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Person", "DefaultPresetId", "dbo.DevicePreset");
            DropIndex("dbo.Person", new[] { "DefaultPresetId" });
            RenameColumn(table: "dbo.DevicePreset", name: "Id", newName: "DevicePresetId");
            RenameIndex(table: "dbo.DevicePreset", name: "IX_Id", newName: "IX_DevicePresetId");
            AddColumn("dbo.Person", "DefaultPreset_DevicePresetId", c => c.Int());
            CreateIndex("dbo.Person", "DefaultPreset_DevicePresetId");
            AddForeignKey("dbo.Person", "DefaultPreset_DevicePresetId", "dbo.DevicePreset", "DevicePresetId");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Person", "DefaultPreset_DevicePresetId", "dbo.DevicePreset");
            DropIndex("dbo.Person", new[] { "DefaultPreset_DevicePresetId" });
            DropColumn("dbo.Person", "DefaultPreset_DevicePresetId");
            RenameIndex(table: "dbo.DevicePreset", name: "IX_DevicePresetId", newName: "IX_Id");
            RenameColumn(table: "dbo.DevicePreset", name: "DevicePresetId", newName: "Id");
            CreateIndex("dbo.Person", "DefaultPresetId");
            AddForeignKey("dbo.Person", "DefaultPresetId", "dbo.DevicePreset", "Id");
        }
    }
}
