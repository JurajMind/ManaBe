namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class namedSetting : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Person", "DefaultSettingId", "dbo.HookahSetting");
            DropIndex("dbo.Person", new[] { "DefaultSettingId" });
            CreateTable(
                "dbo.HookahPersonSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Defaut = c.Boolean(nullable: false),
                        PersonId = c.Int(nullable: false),
                        SettingId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.HookahSetting", t => t.SettingId, cascadeDelete: true)
                .Index(t => t.PersonId)
                .Index(t => t.SettingId);
            
            DropColumn("dbo.Person", "DefaultSettingId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Person", "DefaultSettingId", c => c.Int());
            DropForeignKey("dbo.HookahPersonSetting", "SettingId", "dbo.HookahSetting");
            DropForeignKey("dbo.HookahPersonSetting", "PersonId", "dbo.Person");
            DropIndex("dbo.HookahPersonSetting", new[] { "SettingId" });
            DropIndex("dbo.HookahPersonSetting", new[] { "PersonId" });
            DropTable("dbo.HookahPersonSetting");
            CreateIndex("dbo.Person", "DefaultSettingId");
            AddForeignKey("dbo.Person", "DefaultSettingId", "dbo.HookahSetting", "Id");
        }
    }
}
