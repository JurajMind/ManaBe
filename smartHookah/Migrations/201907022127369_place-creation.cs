namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placecreation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "CreatorId", c => c.Int());
            AddColumn("dbo.Place", "CreatedAt", c => c.DateTime());
            CreateIndex("dbo.Place", "CreatorId");
            AddForeignKey("dbo.Place", "CreatorId", "dbo.Person", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Place", "CreatorId", "dbo.Person");
            DropIndex("dbo.Place", new[] { "CreatorId" });
            DropColumn("dbo.Place", "CreatedAt");
            DropColumn("dbo.Place", "CreatorId");
        }
    }
}
