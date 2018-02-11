namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ssTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "Version", c => c.Binary());
            AddColumn("dbo.SmokeSession", "CreatedAt", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.SmokeSession", "UpdatedAt", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.SmokeSession", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmokeSession", "Deleted");
            DropColumn("dbo.SmokeSession", "UpdatedAt");
            DropColumn("dbo.SmokeSession", "CreatedAt");
            DropColumn("dbo.SmokeSession", "Version");
        }
    }
}
