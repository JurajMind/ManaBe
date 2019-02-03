namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mixPart : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TobacoMixPart", new[] { "TobaccoMix_Id" });
            RenameColumn(table: "dbo.TobacoMixPart", name: "TobaccoMix_Id", newName: "InMixId");
            AlterColumn("dbo.TobacoMixPart", "InMixId", c => c.Int(nullable: false));
            CreateIndex("dbo.TobacoMixPart", "InMixId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TobacoMixPart", new[] { "InMixId" });
            AlterColumn("dbo.TobacoMixPart", "InMixId", c => c.Int());
            RenameColumn(table: "dbo.TobacoMixPart", name: "InMixId", newName: "TobaccoMix_Id");
            CreateIndex("dbo.TobacoMixPart", "TobaccoMix_Id");
        }
    }
}
