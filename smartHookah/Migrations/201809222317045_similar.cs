namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class similar : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SimilarAccesories", "PersonId", "dbo.Person");
            DropIndex("dbo.SimilarAccesories", new[] { "PersonId" });
            AlterColumn("dbo.SimilarAccesories", "PersonId", c => c.Int());
            CreateIndex("dbo.SimilarAccesories", "PersonId");
            AddForeignKey("dbo.SimilarAccesories", "PersonId", "dbo.Person", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SimilarAccesories", "PersonId", "dbo.Person");
            DropIndex("dbo.SimilarAccesories", new[] { "PersonId" });
            AlterColumn("dbo.SimilarAccesories", "PersonId", c => c.Int(nullable: false));
            CreateIndex("dbo.SimilarAccesories", "PersonId");
            AddForeignKey("dbo.SimilarAccesories", "PersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
    }
}
