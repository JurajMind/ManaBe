namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update21 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Update", "Path", c => c.String());
            DropColumn("dbo.Update", "PathTo32");
            DropColumn("dbo.Update", "PathTo60");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Update", "PathTo60", c => c.String());
            AddColumn("dbo.Update", "PathTo32", c => c.String());
            DropColumn("dbo.Update", "Path");
        }
    }
}
