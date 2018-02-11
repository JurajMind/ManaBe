namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class picture : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StandPicture",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PictueString = c.String(),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            Sql("INSERT INTO dbo.StandPicture(PictueString,Width,Height) VALUES('a',1,1)");
            AddColumn("dbo.HookahSetting", "PictureId", c => c.Int(nullable: false,defaultValueSql:"1"));
            CreateIndex("dbo.HookahSetting", "PictureId");
            AddForeignKey("dbo.HookahSetting", "PictureId", "dbo.StandPicture", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HookahSetting", "PictureId", "dbo.StandPicture");
            DropIndex("dbo.HookahSetting", new[] { "PictureId" });
            DropColumn("dbo.HookahSetting", "PictureId");
            DropTable("dbo.StandPicture");
        }
    }
}
