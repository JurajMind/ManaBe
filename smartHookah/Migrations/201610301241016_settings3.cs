namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class settings3 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DbPuf", new[] { "SmokeSession_Id1" });
            DropColumn("dbo.DbPuf", "SmokeSession_Id");
            RenameColumn(table: "dbo.DbPuf", name: "SmokeSession_Id1", newName: "SmokeSession_Id");
            AddColumn("dbo.SmokeSessionStatistics", "Start", c => c.DateTime(nullable: false));
            AddColumn("dbo.SmokeSessionStatistics", "End", c => c.DateTime(nullable: false));
            AddColumn("dbo.SmokeSessionStatistics", "EstimatedPersonCount", c => c.Int(nullable: false));
            AlterColumn("dbo.DbPuf", "SmokeSession_Id", c => c.Int());
            CreateIndex("dbo.DbPuf", "SmokeSession_Id");
            DropColumn("dbo.SmokeSessionStatistics", "SessionStart");
            DropColumn("dbo.SmokeSessionStatistics", "SessionDuration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SmokeSessionStatistics", "SessionDuration", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.SmokeSessionStatistics", "SessionStart", c => c.DateTime(nullable: false));
            DropIndex("dbo.DbPuf", new[] { "SmokeSession_Id" });
            AlterColumn("dbo.DbPuf", "SmokeSession_Id", c => c.Int(nullable: false));
            DropColumn("dbo.SmokeSessionStatistics", "EstimatedPersonCount");
            DropColumn("dbo.SmokeSessionStatistics", "End");
            DropColumn("dbo.SmokeSessionStatistics", "Start");
            RenameColumn(table: "dbo.DbPuf", name: "SmokeSession_Id", newName: "SmokeSession_Id1");
            AddColumn("dbo.DbPuf", "SmokeSession_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.DbPuf", "SmokeSession_Id1");
        }
    }
}
