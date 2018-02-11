namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HookahOrder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlaceId = c.Int(nullable: false),
                        PersonId = c.Int(),
                        SmokeSessionId = c.Int(nullable: false),
                        ExtraInfo = c.String(maxLength: 255),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .ForeignKey("dbo.SmokeSession", t => t.SmokeSessionId, cascadeDelete: true)
                .Index(t => t.PlaceId)
                .Index(t => t.PersonId)
                .Index(t => t.SmokeSessionId);
            
            AddColumn("dbo.Place", "PhoneNumber", c => c.String());
            AddColumn("dbo.Place", "Facebook", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HookahOrder", "SmokeSessionId", "dbo.SmokeSession");
            DropForeignKey("dbo.HookahOrder", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.HookahOrder", "PersonId", "dbo.Person");
            DropIndex("dbo.HookahOrder", new[] { "SmokeSessionId" });
            DropIndex("dbo.HookahOrder", new[] { "PersonId" });
            DropIndex("dbo.HookahOrder", new[] { "PlaceId" });
            DropColumn("dbo.Place", "Facebook");
            DropColumn("dbo.Place", "PhoneNumber");
            DropTable("dbo.HookahOrder");
        }
    }
}
