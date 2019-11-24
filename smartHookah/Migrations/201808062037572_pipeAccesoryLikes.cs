namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class pipeAccesoryLikes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PersonPipeAccesory", "Person_Id", "dbo.Person");
            DropForeignKey("dbo.PersonPipeAccesory", "PipeAccesory_Id", "dbo.PipeAccesory");
            DropIndex("dbo.PersonPipeAccesory", new[] { "Person_Id" });
            DropIndex("dbo.PersonPipeAccesory", new[] { "PipeAccesory_Id" });
            CreateTable(
                "dbo.PipeAccesoryLike",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    PersonId = c.Int(nullable: false),
                    PipeAccesoryId = c.Int(nullable: false),
                    Value = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.PipeAccesory", t => t.PipeAccesoryId, cascadeDelete: true)
                .Index(t => t.PersonId)
                .Index(t => t.PipeAccesoryId);

            AddColumn("dbo.PipeAccesory", "LikeCount", c => c.Int(nullable: false));
            AddColumn("dbo.PipeAccesory", "DisLikeCount", c => c.Int(nullable: false));
            DropTable("dbo.PersonPipeAccesory");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.PersonPipeAccesory",
                c => new
                {
                    Person_Id = c.Int(nullable: false),
                    PipeAccesory_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Person_Id, t.PipeAccesory_Id });

            DropForeignKey("dbo.PipeAccesoryLike", "PipeAccesoryId", "dbo.PipeAccesory");
            DropForeignKey("dbo.PipeAccesoryLike", "PersonId", "dbo.Person");
            DropIndex("dbo.PipeAccesoryLike", new[] { "PipeAccesoryId" });
            DropIndex("dbo.PipeAccesoryLike", new[] { "PersonId" });
            DropColumn("dbo.PipeAccesory", "DisLikeCount");
            DropColumn("dbo.PipeAccesory", "LikeCount");
            DropTable("dbo.PipeAccesoryLike");
            CreateIndex("dbo.PersonPipeAccesory", "PipeAccesory_Id");
            CreateIndex("dbo.PersonPipeAccesory", "Person_Id");
            AddForeignKey("dbo.PersonPipeAccesory", "PipeAccesory_Id", "dbo.PipeAccesory", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PersonPipeAccesory", "Person_Id", "dbo.Person", "Id", cascadeDelete: true);
        }
    }
}
