namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gamefix6 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.EventProgress");
            AddColumn("dbo.EventProgress", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.EventProgress", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.EventProgress");
            DropColumn("dbo.EventProgress", "Id");
            AddPrimaryKey("dbo.EventProgress", new[] { "GameProfileId", "EventId" });
        }
    }
}
