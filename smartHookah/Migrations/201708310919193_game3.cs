namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Event", "Experience", c => c.Int(nullable: false));
            AddColumn("dbo.Event", "Clouds", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Event", "Clouds");
            DropColumn("dbo.Event", "Experience");
        }
    }
}
