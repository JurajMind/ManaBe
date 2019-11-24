namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class translation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BrandTranslation",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Lng = c.String(nullable: false, maxLength: 2),
                    Descriptions = c.String(),
                    ShortDescriptions = c.String(maxLength: 255),
                    Created = c.DateTime(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    Updated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.Lng })
                .ForeignKey("dbo.Brand", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);

            CreateTable(
                "dbo.PlaceTranslation",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Lng = c.String(nullable: false, maxLength: 2),
                    Descriptions = c.String(),
                    ShortDescriptions = c.String(maxLength: 255),
                    Created = c.DateTime(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    Updated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.Lng })
                .ForeignKey("dbo.Place", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);

            CreateTable(
                "dbo.FeatureMixCreatorTranslation",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Lng = c.String(nullable: false, maxLength: 2),
                    Descriptions = c.String(),
                    ShortDescriptions = c.String(maxLength: 255),
                    Created = c.DateTime(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    Updated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.Id, t.Lng })
                .ForeignKey("dbo.FeatureMixCreator", t => t.Id, cascadeDelete: true)
                .Index(t => t.Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.FeatureMixCreatorTranslation", "Id", "dbo.FeatureMixCreator");
            DropForeignKey("dbo.PlaceTranslation", "Id", "dbo.Place");
            DropForeignKey("dbo.BrandTranslation", "Id", "dbo.Brand");
            DropIndex("dbo.FeatureMixCreatorTranslation", new[] { "Id" });
            DropIndex("dbo.PlaceTranslation", new[] { "Id" });
            DropIndex("dbo.BrandTranslation", new[] { "Id" });
            DropTable("dbo.FeatureMixCreatorTranslation");
            DropTable("dbo.PlaceTranslation");
            DropTable("dbo.BrandTranslation");
        }
    }
}
