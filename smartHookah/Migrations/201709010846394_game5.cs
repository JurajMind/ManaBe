namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game5 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventRecepie", "Event_Id", "dbo.Event");
            DropIndex("dbo.EventRecepie", new[] { "Event_Id" });
            DropPrimaryKey("dbo.EventRecepie");
            DropColumn("dbo.EventRecepie", "EventId");
            RenameColumn(table: "dbo.EventRecepie", name: "Event_Id", newName: "EventId");
            AddColumn("dbo.EventRecepie", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventRecepie", "EventId", c => c.Int(nullable: false));
            AlterColumn("dbo.EventRecepie", "EventId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.EventRecepie", "Id");
            CreateIndex("dbo.EventRecepie", "EventId");
            AddForeignKey("dbo.EventRecepie", "EventId", "dbo.Event", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventRecepie", "EventId", "dbo.Event");
            DropIndex("dbo.EventRecepie", new[] { "EventId" });
            DropPrimaryKey("dbo.EventRecepie");
            AlterColumn("dbo.EventRecepie", "EventId", c => c.Int());
            AlterColumn("dbo.EventRecepie", "EventId", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.EventRecepie", "Id");
            AddPrimaryKey("dbo.EventRecepie", "EventId");
            RenameColumn(table: "dbo.EventRecepie", name: "EventId", newName: "Event_Id");
            AddColumn("dbo.EventRecepie", "EventId", c => c.Int(nullable: false, identity: true));
            CreateIndex("dbo.EventRecepie", "Event_Id");
            AddForeignKey("dbo.EventRecepie", "Event_Id", "dbo.Event", "Id");
        }
    }
}
