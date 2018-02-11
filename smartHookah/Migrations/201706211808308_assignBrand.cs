namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class assignBrand : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "AssignedBrandId", c => c.String(maxLength: 128));
            CreateIndex("dbo.Person", "AssignedBrandId");
            AddForeignKey("dbo.Person", "AssignedBrandId", "dbo.Brand", "Name");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Person", "AssignedBrandId", "dbo.Brand");
            DropIndex("dbo.Person", new[] { "AssignedBrandId" });
            DropColumn("dbo.Person", "AssignedBrandId");
        }
    }
}
