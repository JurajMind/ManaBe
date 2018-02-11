namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class share : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "Token", c => c.String(maxLength: 10));
            CreateIndex("dbo.SmokeSession", "Token", name: "Token");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SmokeSession", "Token");
            DropColumn("dbo.SmokeSession", "Token");
        }
    }
}
