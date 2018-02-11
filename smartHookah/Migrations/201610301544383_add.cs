namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge");
            DropIndex("dbo.PipeAccesory", new[] { "TobaccoMix_Id" });
            DropIndex("dbo.Hookah", new[] { "LoungeId" });
            CreateTable(
                "dbo.TobacoMixPart",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Fraction = c.Double(nullable: false),
                        Tobacco_Id = c.Int(),
                        TobaccoMix_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PipeAccesory", t => t.Tobacco_Id)
                .Index(t => t.Tobacco_Id)
                .Index(t => t.TobaccoMix_Id);
            
            AddColumn("dbo.PipeAccesory", "MixName", c => c.String());
            AlterColumn("dbo.Hookah", "LoungeId", c => c.Int());
            CreateIndex("dbo.Hookah", "LoungeId");
            AddForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge", "Id");
            DropColumn("dbo.PipeAccesory", "TobaccoMix_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesory", "TobaccoMix_Id", c => c.Int());
            DropForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge");
            DropForeignKey("dbo.TobacoMixPart", "Tobacco_Id", "dbo.PipeAccesory");
            DropIndex("dbo.Hookah", new[] { "LoungeId" });
            DropIndex("dbo.TobacoMixPart", new[] { "TobaccoMix_Id" });
            DropIndex("dbo.TobacoMixPart", new[] { "Tobacco_Id" });
            AlterColumn("dbo.Hookah", "LoungeId", c => c.Int(nullable: false));
            DropColumn("dbo.PipeAccesory", "MixName");
            DropTable("dbo.TobacoMixPart");
            CreateIndex("dbo.Hookah", "LoungeId");
            CreateIndex("dbo.PipeAccesory", "TobaccoMix_Id");
            AddForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge", "Id", cascadeDelete: true);
        }
    }
}
