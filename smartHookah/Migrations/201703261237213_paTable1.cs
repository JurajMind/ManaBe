namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class paTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "Version", c => c.Binary());
            AddColumn("dbo.PipeAccesory", "CreatedAt", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.PipeAccesory", "UpdatedAt", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.PipeAccesory", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PipeAccesory", "Deleted");
            DropColumn("dbo.PipeAccesory", "UpdatedAt");
            DropColumn("dbo.PipeAccesory", "CreatedAt");
            DropColumn("dbo.PipeAccesory", "Version");
        }
    }
}
