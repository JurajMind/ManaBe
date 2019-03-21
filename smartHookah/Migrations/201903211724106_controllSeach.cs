namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class controllSeach : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "ControlSearch", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PipeAccesory", "ControlSearch");
        }
    }
}
