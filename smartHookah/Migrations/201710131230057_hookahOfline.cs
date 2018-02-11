namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hookahOfline : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hookah", "Offline", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hookah", "Offline");
        }
    }
}
