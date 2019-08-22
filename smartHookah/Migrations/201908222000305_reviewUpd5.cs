namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewUpd5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SessionReview", "SmokeSessionId", "dbo.SmokeSession");
            DropIndex("dbo.SessionReview", new[] { "SmokeSessionId" });
            RenameColumn(table: "dbo.SessionReview", name: "SmokeSessionId", newName: "SmokeSession_Id");
            AlterColumn("dbo.SessionReview", "SmokeSession_Id", c => c.Int());
            CreateIndex("dbo.SessionReview", "SmokeSession_Id");
            AddForeignKey("dbo.SessionReview", "SmokeSession_Id", "dbo.SmokeSession", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SessionReview", "SmokeSession_Id", "dbo.SmokeSession");
            DropIndex("dbo.SessionReview", new[] { "SmokeSession_Id" });
            AlterColumn("dbo.SessionReview", "SmokeSession_Id", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.SessionReview", name: "SmokeSession_Id", newName: "SmokeSessionId");
            CreateIndex("dbo.SessionReview", "SmokeSessionId");
            AddForeignKey("dbo.SessionReview", "SmokeSessionId", "dbo.SmokeSession", "Id", cascadeDelete: true);
        }
    }
}
