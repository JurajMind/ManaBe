namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tag : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SmokeSessionMetadataTags",
                c => new
                    {
                        SmokeSessionMetadataRefId = c.Int(nullable: false),
                        TagRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SmokeSessionMetadataRefId, t.TagRefId })
                .ForeignKey("dbo.SmokeSessionMetaData", t => t.SmokeSessionMetadataRefId, cascadeDelete: true)
                .ForeignKey("dbo.Tag", t => t.TagRefId, cascadeDelete: true)
                .Index(t => t.SmokeSessionMetadataRefId)
                .Index(t => t.TagRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmokeSessionMetadataTags", "TagRefId", "dbo.Tag");
            DropForeignKey("dbo.SmokeSessionMetadataTags", "SmokeSessionMetadataRefId", "dbo.SmokeSessionMetaData");
            DropIndex("dbo.SmokeSessionMetadataTags", new[] { "TagRefId" });
            DropIndex("dbo.SmokeSessionMetadataTags", new[] { "SmokeSessionMetadataRefId" });
            DropTable("dbo.SmokeSessionMetadataTags");
            DropTable("dbo.Tag");
        }
    }
}
