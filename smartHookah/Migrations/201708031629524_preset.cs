namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class preset : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "PresetId", c => c.Int());
            CreateIndex("dbo.Person", "PresetId");
            AddForeignKey("dbo.Person", "PresetId", "dbo.SmokeSessionMetaData", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Person", "PresetId", "dbo.SmokeSessionMetaData");
            DropIndex("dbo.Person", new[] { "PresetId" });
            DropColumn("dbo.Person", "PresetId");
        }
    }
}
