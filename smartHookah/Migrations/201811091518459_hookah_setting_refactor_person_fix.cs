namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookah_setting_refactor_person_fix : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DevicePreset", new[] { "Person_Id" });
            DropColumn("dbo.DevicePreset", "PersonId");
            RenameColumn(table: "dbo.DevicePreset", name: "Person_Id", newName: "PersonId");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.DevicePreset", name: "PersonId", newName: "Person_Id");
            AddColumn("dbo.DevicePreset", "PersonId", c => c.Int());
            CreateIndex("dbo.DevicePreset", "Person_Id");
        }
    }
}
