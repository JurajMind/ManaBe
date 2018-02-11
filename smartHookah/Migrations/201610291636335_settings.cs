namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class settings : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Hookah", new[] { "Code" });
            CreateTable(
                "dbo.HookahSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Hookah", "SettingId", c => c.Int());
            AlterColumn("dbo.Hookah", "Code", c => c.String(maxLength: 128));
            CreateIndex("dbo.Hookah", "Code", unique: true);
            CreateIndex("dbo.Hookah", "SettingId");
            AddForeignKey("dbo.Hookah", "SettingId", "dbo.HookahSetting", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Hookah", "SettingId", "dbo.HookahSetting");
            DropIndex("dbo.Hookah", new[] { "SettingId" });
            DropIndex("dbo.Hookah", new[] { "Code" });
            AlterColumn("dbo.Hookah", "Code", c => c.String(maxLength: 20));
            DropColumn("dbo.Hookah", "SettingId");
            DropTable("dbo.HookahSetting");
            CreateIndex("dbo.Hookah", "Code", unique: true);
        }
    }
}
