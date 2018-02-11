namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UpdateLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HookahId = c.Int(nullable: false),
                        OldVersion = c.Int(nullable: false),
                        NewVersionId = c.Int(nullable: false),
                        UpdateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Hookah", t => t.HookahId, cascadeDelete: true)
                .ForeignKey("dbo.Update", t => t.NewVersionId, cascadeDelete: true)
                .Index(t => t.HookahId)
                .Index(t => t.NewVersionId);
            
            CreateTable(
                "dbo.Update",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Version = c.Int(nullable: false),
                        ReleseDate = c.String(),
                        ReleseNote = c.String(),
                        Type = c.Int(nullable: false),
                        PathTo32 = c.String(),
                        PathTo60 = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Hookah", "Type", c => c.Int(nullable: false,defaultValueSql: "0"));
            AddColumn("dbo.Hookah", "UpdateType", c => c.Int(nullable: false,defaultValueSql:"0"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UpdateLog", "NewVersionId", "dbo.Update");
            DropForeignKey("dbo.UpdateLog", "HookahId", "dbo.Hookah");
            DropIndex("dbo.UpdateLog", new[] { "NewVersionId" });
            DropIndex("dbo.UpdateLog", new[] { "HookahId" });
            DropColumn("dbo.Hookah", "UpdateType");
            DropColumn("dbo.Hookah", "Type");
            DropTable("dbo.Update");
            DropTable("dbo.UpdateLog");
        }
    }
}
