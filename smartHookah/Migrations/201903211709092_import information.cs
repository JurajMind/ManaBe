namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class importinformation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ImportInformation",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DateTimeCreatedAt = c.DateTime(nullable: false),
                    DataSource = c.String(),
                    DataPath = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.AccessoryImportMapping",
                c => new
                {
                    AccessoryRefId = c.Int(nullable: false),
                    ImportRefId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.AccessoryRefId, t.ImportRefId })
                .ForeignKey("dbo.PipeAccesory", t => t.AccessoryRefId, cascadeDelete: true)
                .ForeignKey("dbo.ImportInformation", t => t.ImportRefId, cascadeDelete: true)
                .Index(t => t.AccessoryRefId)
                .Index(t => t.ImportRefId);

            AddColumn("dbo.PipeAccesory", "Valid", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropForeignKey("dbo.AccessoryImportMapping", "ImportRefId", "dbo.ImportInformation");
            DropForeignKey("dbo.AccessoryImportMapping", "AccessoryRefId", "dbo.PipeAccesory");
            DropIndex("dbo.AccessoryImportMapping", new[] { "ImportRefId" });
            DropIndex("dbo.AccessoryImportMapping", new[] { "AccessoryRefId" });
            DropColumn("dbo.PipeAccesory", "Valid");
            DropTable("dbo.AccessoryImportMapping");
            DropTable("dbo.ImportInformation");
        }
    }
}
