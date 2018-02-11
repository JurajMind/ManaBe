namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class urlIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Place", new[] { "FriendlyUrl" });
            CreateIndex("dbo.Place", "FriendlyUrl", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Place", new[] { "FriendlyUrl" });
            CreateIndex("dbo.Place", "FriendlyUrl");
        }
    }
}
