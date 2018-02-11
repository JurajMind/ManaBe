namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game61 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DoneEventPerson", "PersonRefId", "dbo.GameProfile");
            DropForeignKey("dbo.DoneEventPerson", "EventRefId", "dbo.Event");
            DropIndex("dbo.DoneEventPerson", new[] { "PersonRefId" });
            DropIndex("dbo.DoneEventPerson", new[] { "EventRefId" });
            CreateTable(
                "dbo.DoneEvent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GameProfileId = c.Int(nullable: false),
                        EventId = c.Int(nullable: false),
                        Obtain = c.DateTime(nullable: false),
                        GameProfile_PersonId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Event", t => t.EventId, cascadeDelete: true)
                .ForeignKey("dbo.GameProfile", t => t.GameProfile_PersonId)
                .Index(t => t.EventId)
                .Index(t => t.GameProfile_PersonId);
            
            AddColumn("dbo.GameProfile", "Event_Id", c => c.Int());
            CreateIndex("dbo.GameProfile", "Event_Id");
            AddForeignKey("dbo.GameProfile", "Event_Id", "dbo.Event", "Id");
            DropTable("dbo.DoneEventPerson");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DoneEventPerson",
                c => new
                    {
                        PersonRefId = c.Int(nullable: false),
                        EventRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PersonRefId, t.EventRefId });
            
            DropForeignKey("dbo.DoneEvent", "GameProfile_PersonId", "dbo.GameProfile");
            DropForeignKey("dbo.DoneEvent", "EventId", "dbo.Event");
            DropForeignKey("dbo.GameProfile", "Event_Id", "dbo.Event");
            DropIndex("dbo.DoneEvent", new[] { "GameProfile_PersonId" });
            DropIndex("dbo.DoneEvent", new[] { "EventId" });
            DropIndex("dbo.GameProfile", new[] { "Event_Id" });
            DropColumn("dbo.GameProfile", "Event_Id");
            DropTable("dbo.DoneEvent");
            CreateIndex("dbo.DoneEventPerson", "EventRefId");
            CreateIndex("dbo.DoneEventPerson", "PersonRefId");
            AddForeignKey("dbo.DoneEventPerson", "EventRefId", "dbo.Event", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DoneEventPerson", "PersonRefId", "dbo.GameProfile", "PersonId", cascadeDelete: true);
        }
    }
}
