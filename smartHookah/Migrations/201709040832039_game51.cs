namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game51 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventProgress",
                c => new
                    {
                        GameProfileId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                        IntProgress = c.Int(nullable: false),
                        TimeProgress = c.Time(nullable: false, precision: 7),
                        StringProgress = c.String(),
                        GameProfile_PersonId = c.Int(),
                    })
                .PrimaryKey(t => new { t.GameProfileId, t.EventId })
                .ForeignKey("dbo.Event", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.GameProfile", t => t.GameProfile_PersonId)
                .Index(t => t.EventId)
                .Index(t => t.GameProfile_PersonId);
            
            CreateTable(
                "dbo.Notificatiom",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(),
                        DateTime = c.DateTime(nullable: false),
                        Read = c.Boolean(nullable: false),
                        Msg = c.String(),
                        Type = c.Int(nullable: false),
                        JumpUrl = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId);
            
            AddColumn("dbo.Event", "TriggerCount", c => c.Int(nullable: false));
            AddColumn("dbo.Event", "TriggerTime", c => c.Time(nullable: false, precision: 7));
            DropColumn("dbo.EventRecepie", "TriggerCount");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventRecepie", "TriggerCount", c => c.Int(nullable: false));
            DropForeignKey("dbo.Notificatiom", "PersonId", "dbo.Person");
            DropForeignKey("dbo.EventProgress", "GameProfile_PersonId", "dbo.GameProfile");
            DropForeignKey("dbo.EventProgress", "EventId", "dbo.Event");
            DropIndex("dbo.Notificatiom", new[] { "PersonId" });
            DropIndex("dbo.EventProgress", new[] { "GameProfile_PersonId" });
            DropIndex("dbo.EventProgress", new[] { "EventId" });
            DropColumn("dbo.Event", "TriggerTime");
            DropColumn("dbo.Event", "TriggerCount");
            DropTable("dbo.Notificatiom");
            DropTable("dbo.EventProgress");
        }
    }
}
