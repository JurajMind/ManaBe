namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class accesoryCreation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "CreatorId", c => c.Int());
            AddColumn("dbo.PipeAccesory", "Status", c => c.Int(nullable: false));
            CreateIndex("dbo.PipeAccesory", "CreatorId");
            AddForeignKey("dbo.PipeAccesory", "CreatorId", "dbo.Person", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.PipeAccesory", "CreatorId", "dbo.Person");
            DropIndex("dbo.PipeAccesory", new[] { "CreatorId" });
            DropColumn("dbo.PipeAccesory", "Status");
            DropColumn("dbo.PipeAccesory", "CreatorId");
        }
    }
}
