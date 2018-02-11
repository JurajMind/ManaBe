namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orderMetadata : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HookahOrder", "SmokeSessionId", "dbo.SmokeSession");
            DropIndex("dbo.HookahOrder", new[] { "SmokeSessionId" });
            AddColumn("dbo.HookahOrder", "SmokeSessionMetaDataId", c => c.Int(nullable: false));
            AlterColumn("dbo.HookahOrder", "SmokeSessionId", c => c.Int());
            CreateIndex("dbo.HookahOrder", "SmokeSessionMetaDataId");
            CreateIndex("dbo.HookahOrder", "SmokeSessionId");
            AddForeignKey("dbo.HookahOrder", "SmokeSessionMetaDataId", "dbo.SmokeSessionMetaData", "Id", cascadeDelete: true);
            AddForeignKey("dbo.HookahOrder", "SmokeSessionId", "dbo.SmokeSession", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HookahOrder", "SmokeSessionId", "dbo.SmokeSession");
            DropForeignKey("dbo.HookahOrder", "SmokeSessionMetaDataId", "dbo.SmokeSessionMetaData");
            DropIndex("dbo.HookahOrder", new[] { "SmokeSessionId" });
            DropIndex("dbo.HookahOrder", new[] { "SmokeSessionMetaDataId" });
            AlterColumn("dbo.HookahOrder", "SmokeSessionId", c => c.Int(nullable: false));
            DropColumn("dbo.HookahOrder", "SmokeSessionMetaDataId");
            CreateIndex("dbo.HookahOrder", "SmokeSessionId");
            AddForeignKey("dbo.HookahOrder", "SmokeSessionId", "dbo.SmokeSession", "Id", cascadeDelete: true);
        }
    }
}
