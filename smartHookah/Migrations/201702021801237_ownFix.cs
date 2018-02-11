namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ownFix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Person", "Hookah_Id", "dbo.Hookah");
            DropIndex("dbo.Person", new[] { "Hookah_Id" });
            CreateTable(
                "dbo.HookahOwning",
                c => new
                    {
                        HookahRefId = c.Int(nullable: false),
                        PersonRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.HookahRefId, t.PersonRefId })
                .ForeignKey("dbo.Person", t => t.HookahRefId, cascadeDelete: true)
                .ForeignKey("dbo.Hookah", t => t.PersonRefId, cascadeDelete: true)
                .Index(t => t.HookahRefId)
                .Index(t => t.PersonRefId);
            
            DropColumn("dbo.Person", "Hookah_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Person", "Hookah_Id", c => c.Int());
            DropForeignKey("dbo.HookahOwning", "PersonRefId", "dbo.Hookah");
            DropForeignKey("dbo.HookahOwning", "HookahRefId", "dbo.Person");
            DropIndex("dbo.HookahOwning", new[] { "PersonRefId" });
            DropIndex("dbo.HookahOwning", new[] { "HookahRefId" });
            DropTable("dbo.HookahOwning");
            CreateIndex("dbo.Person", "Hookah_Id");
            AddForeignKey("dbo.Person", "Hookah_Id", "dbo.Hookah", "Id");
        }
    }
}
