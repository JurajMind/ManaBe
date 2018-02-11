namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tobaccoTaste : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TobaccoTaste",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OriginalName = c.String(),
                        EngName = c.String(),
                        CzName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TobaccoTasteBinding",
                c => new
                    {
                        TobaccoRefId = c.Int(nullable: false),
                        TasteRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TobaccoRefId, t.TasteRefId })
                .ForeignKey("dbo.PipeAccesory", t => t.TobaccoRefId, cascadeDelete: true)
                .ForeignKey("dbo.TobaccoTaste", t => t.TasteRefId, cascadeDelete: true)
                .Index(t => t.TobaccoRefId)
                .Index(t => t.TasteRefId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TobaccoTasteBinding", "TasteRefId", "dbo.TobaccoTaste");
            DropForeignKey("dbo.TobaccoTasteBinding", "TobaccoRefId", "dbo.PipeAccesory");
            DropIndex("dbo.TobaccoTasteBinding", new[] { "TasteRefId" });
            DropIndex("dbo.TobaccoTasteBinding", new[] { "TobaccoRefId" });
            DropTable("dbo.TobaccoTasteBinding");
            DropTable("dbo.TobaccoTaste");
        }
    }
}
