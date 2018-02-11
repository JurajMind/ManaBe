namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameProfile",
                c => new
                    {
                        PersonId = c.Int(nullable: false),
                        Level = c.Int(nullable: false),
                        Experience = c.Long(nullable: false),
                        Clouds = c.Int(nullable: false),
                        TitleId = c.Int(),
                        Badge1Id = c.Int(),
                        Badge2Id = c.Int(),
                        Badge3Id = c.Int(),
                        Badge4Id = c.Int(),
                    })
                .PrimaryKey(t => t.PersonId)
                .ForeignKey("dbo.Reward", t => t.Badge1Id)
                .ForeignKey("dbo.Reward", t => t.Badge2Id)
                .ForeignKey("dbo.Reward", t => t.Badge3Id)
                .ForeignKey("dbo.Reward", t => t.Badge4Id)
                .ForeignKey("dbo.Reward", t => t.TitleId)
                .ForeignKey("dbo.Person", t => t.PersonId)
                .Index(t => t.PersonId)
                .Index(t => t.TitleId)
                .Index(t => t.Badge1Id)
                .Index(t => t.Badge2Id)
                .Index(t => t.Badge3Id)
                .Index(t => t.Badge4Id);
            
            CreateTable(
                "dbo.Reward",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        EventId = c.Int(),
                        Param = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Event", t => t.EventId)
                .Index(t => t.EventId);
            
            CreateTable(
                "dbo.Event",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        Description = c.String(),
                        From = c.DateTime(),
                        To = c.DateTime(),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventRecepie",
                c => new
                    {
                        EventId = c.Int(nullable: false, identity: true),
                        EventString = c.String(),
                        EventForeinId = c.Int(nullable: false),
                        EventTimeSpan = c.Time(nullable: false, precision: 7),
                        Active = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                        Event_Id = c.Int(),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.Event", t => t.Event_Id)
                .Index(t => t.Event_Id);
            
            CreateTable(
                "dbo.DoneEventPerson",
                c => new
                    {
                        PersonRefId = c.Int(nullable: false),
                        EventRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PersonRefId, t.EventRefId })
                .ForeignKey("dbo.GameProfile", t => t.PersonRefId, cascadeDelete: true)
                .ForeignKey("dbo.Event", t => t.EventRefId, cascadeDelete: true)
                .Index(t => t.PersonRefId)
                .Index(t => t.EventRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameProfile", "PersonId", "dbo.Person");
            DropForeignKey("dbo.GameProfile", "TitleId", "dbo.Reward");
            DropForeignKey("dbo.DoneEventPerson", "EventRefId", "dbo.Event");
            DropForeignKey("dbo.DoneEventPerson", "PersonRefId", "dbo.GameProfile");
            DropForeignKey("dbo.GameProfile", "Badge4Id", "dbo.Reward");
            DropForeignKey("dbo.GameProfile", "Badge3Id", "dbo.Reward");
            DropForeignKey("dbo.GameProfile", "Badge2Id", "dbo.Reward");
            DropForeignKey("dbo.GameProfile", "Badge1Id", "dbo.Reward");
            DropForeignKey("dbo.Reward", "EventId", "dbo.Event");
            DropForeignKey("dbo.EventRecepie", "Event_Id", "dbo.Event");
            DropIndex("dbo.DoneEventPerson", new[] { "EventRefId" });
            DropIndex("dbo.DoneEventPerson", new[] { "PersonRefId" });
            DropIndex("dbo.EventRecepie", new[] { "Event_Id" });
            DropIndex("dbo.Reward", new[] { "EventId" });
            DropIndex("dbo.GameProfile", new[] { "Badge4Id" });
            DropIndex("dbo.GameProfile", new[] { "Badge3Id" });
            DropIndex("dbo.GameProfile", new[] { "Badge2Id" });
            DropIndex("dbo.GameProfile", new[] { "Badge1Id" });
            DropIndex("dbo.GameProfile", new[] { "TitleId" });
            DropIndex("dbo.GameProfile", new[] { "PersonId" });
            DropTable("dbo.DoneEventPerson");
            DropTable("dbo.EventRecepie");
            DropTable("dbo.Event");
            DropTable("dbo.Reward");
            DropTable("dbo.GameProfile");
        }
    }
}
