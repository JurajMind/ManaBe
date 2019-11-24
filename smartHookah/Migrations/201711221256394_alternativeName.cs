namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class alternativeName : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SimilarAccesories",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    OriginalId = c.Int(nullable: false),
                    SimilarId = c.Int(nullable: false),
                    PersonId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.PipeAccesory", t => t.SimilarId, cascadeDelete: true)
                .ForeignKey("dbo.PipeAccesory", t => t.OriginalId)
                .Index(t => t.OriginalId)
                .Index(t => t.SimilarId)
                .Index(t => t.PersonId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.SimilarAccesories", "OriginalId", "dbo.PipeAccesory");
            DropForeignKey("dbo.SimilarAccesories", "SimilarId", "dbo.PipeAccesory");
            DropForeignKey("dbo.SimilarAccesories", "PersonId", "dbo.Person");
            DropIndex("dbo.SimilarAccesories", new[] { "PersonId" });
            DropIndex("dbo.SimilarAccesories", new[] { "SimilarId" });
            DropIndex("dbo.SimilarAccesories", new[] { "OriginalId" });
            DropTable("dbo.SimilarAccesories");
        }
    }
}
