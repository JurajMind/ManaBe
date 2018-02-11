namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placeFix4 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Person", "PlaceId", "dbo.Place");
            DropIndex("dbo.Person", new[] { "PlaceId" });
            DropColumn("dbo.Person", "PlaceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Person", "PlaceId", c => c.Int());
            CreateIndex("dbo.Person", "PlaceId");
            AddForeignKey("dbo.Person", "PlaceId", "dbo.Place", "Id");
        }
    }
}
