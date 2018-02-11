namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class defaultValues : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "DefaultSettingId", c => c.Int());
            AddColumn("dbo.Person", "DefaultMetaDataId", c => c.Int());
            AddColumn("dbo.Hookah", "DefaultMetaDataId", c => c.Int());
            CreateIndex("dbo.Person", "DefaultSettingId");
            CreateIndex("dbo.Person", "DefaultMetaDataId");
            CreateIndex("dbo.Hookah", "DefaultMetaDataId");
            AddForeignKey("dbo.Person", "DefaultMetaDataId", "dbo.SmokeSessionMetaData", "Id");
            AddForeignKey("dbo.Person", "DefaultSettingId", "dbo.HookahSetting", "Id");
            AddForeignKey("dbo.Hookah", "DefaultMetaDataId", "dbo.SmokeSessionMetaData", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Hookah", "DefaultMetaDataId", "dbo.SmokeSessionMetaData");
            DropForeignKey("dbo.Person", "DefaultSettingId", "dbo.HookahSetting");
            DropForeignKey("dbo.Person", "DefaultMetaDataId", "dbo.SmokeSessionMetaData");
            DropIndex("dbo.Hookah", new[] { "DefaultMetaDataId" });
            DropIndex("dbo.Person", new[] { "DefaultMetaDataId" });
            DropIndex("dbo.Person", new[] { "DefaultSettingId" });
            DropColumn("dbo.Hookah", "DefaultMetaDataId");
            DropColumn("dbo.Person", "DefaultMetaDataId");
            DropColumn("dbo.Person", "DefaultSettingId");
        }
    }
}
