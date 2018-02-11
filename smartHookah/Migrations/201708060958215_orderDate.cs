namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orderDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahOrder", "Created", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HookahOrder", "Created");
        }
    }
}
