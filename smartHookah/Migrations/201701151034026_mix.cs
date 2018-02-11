namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "AuthorId", c => c.Int());
            AddColumn("dbo.Brand", "TobaccoMixBrand", c => c.Boolean(nullable: false));
            CreateIndex("dbo.PipeAccesory", "AuthorId");
            AddForeignKey("dbo.PipeAccesory", "AuthorId", "dbo.Person", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PipeAccesory", "AuthorId", "dbo.Person");
            DropIndex("dbo.PipeAccesory", new[] { "AuthorId" });
            DropColumn("dbo.Brand", "TobaccoMixBrand");
            DropColumn("dbo.PipeAccesory", "AuthorId");
        }
    }
}
