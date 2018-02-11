namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test4 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TobacoMixPart", "Tobacco_Id", "dbo.PipeAccesory");
            DropIndex("dbo.TobacoMixPart", new[] { "Tobacco_Id" });
            RenameColumn(table: "dbo.TobacoMixPart", name: "Tobacco_Id", newName: "TobaccoId");
            AlterColumn("dbo.TobacoMixPart", "Fraction", c => c.Int(nullable: false));
            AlterColumn("dbo.TobacoMixPart", "TobaccoId", c => c.Int(nullable: false));
            CreateIndex("dbo.TobacoMixPart", "TobaccoId");
            AddForeignKey("dbo.TobacoMixPart", "TobaccoId", "dbo.PipeAccesory", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TobacoMixPart", "TobaccoId", "dbo.PipeAccesory");
            DropIndex("dbo.TobacoMixPart", new[] { "TobaccoId" });
            AlterColumn("dbo.TobacoMixPart", "TobaccoId", c => c.Int());
            AlterColumn("dbo.TobacoMixPart", "Fraction", c => c.Double(nullable: false));
            RenameColumn(table: "dbo.TobacoMixPart", name: "TobaccoId", newName: "Tobacco_Id");
            CreateIndex("dbo.TobacoMixPart", "Tobacco_Id");
            AddForeignKey("dbo.TobacoMixPart", "Tobacco_Id", "dbo.PipeAccesory", "Id");
        }
    }
}
