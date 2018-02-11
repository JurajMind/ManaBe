namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class smokeEvent : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SmokeEvent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SmokeSessionId = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                        NumParam = c.Int(nullable: false),
                        Comment = c.String(maxLength: 255),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SmokeSession", t => t.SmokeSessionId, cascadeDelete: true)
                .Index(t => t.SmokeSessionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmokeEvent", "SmokeSessionId", "dbo.SmokeSession");
            DropIndex("dbo.SmokeEvent", new[] { "SmokeSessionId" });
            DropTable("dbo.SmokeEvent");
        }
    }
}
