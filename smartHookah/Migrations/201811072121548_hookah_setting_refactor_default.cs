namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookah_setting_refactor_default : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DevicePreset", "PersonId", "dbo.Person");
            AddColumn("dbo.Person", "DefaultPresetId", c => c.Int());
            AddColumn("dbo.DevicePreset", "Person_Id", c => c.Int());
            CreateIndex("dbo.Person", "DefaultPresetId");
            CreateIndex("dbo.DevicePreset", "Person_Id");
            AddForeignKey("dbo.Person", "DefaultPresetId", "dbo.DevicePreset", "Id");
            AddForeignKey("dbo.DevicePreset", "Person_Id", "dbo.Person", "Id");
            DropColumn("dbo.DevicePreset", "Defaut");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DevicePreset", "Defaut", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.DevicePreset", "Person_Id", "dbo.Person");
            DropForeignKey("dbo.Person", "DefaultPresetId", "dbo.DevicePreset");
            DropIndex("dbo.DevicePreset", new[] { "Person_Id" });
            DropIndex("dbo.Person", new[] { "DefaultPresetId" });
            DropColumn("dbo.DevicePreset", "Person_Id");
            DropColumn("dbo.Person", "DefaultPresetId");
            AddForeignKey("dbo.DevicePreset", "PersonId", "dbo.Person", "Id");
        }
    }
}
