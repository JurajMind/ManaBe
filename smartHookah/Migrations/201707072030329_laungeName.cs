namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class laungeName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lounge", "Name", c => c.String(nullable: false, maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lounge", "Name");
        }
    }
}
