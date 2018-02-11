namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gamefix7 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EventProgress", "GameProfile_PersonId", "dbo.GameProfile");
            DropIndex("dbo.EventProgress", new[] { "GameProfile_PersonId" });
            DropColumn("dbo.EventProgress", "GameProfileId");
            RenameColumn(table: "dbo.EventProgress", name: "GameProfile_PersonId", newName: "GameProfileId");
            AlterColumn("dbo.EventProgress", "GameProfileId", c => c.Int(nullable: false));
            CreateIndex("dbo.EventProgress", "GameProfileId");
            AddForeignKey("dbo.EventProgress", "GameProfileId", "dbo.GameProfile", "PersonId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventProgress", "GameProfileId", "dbo.GameProfile");
            DropIndex("dbo.EventProgress", new[] { "GameProfileId" });
            AlterColumn("dbo.EventProgress", "GameProfileId", c => c.Int());
            RenameColumn(table: "dbo.EventProgress", name: "GameProfileId", newName: "GameProfile_PersonId");
            AddColumn("dbo.EventProgress", "GameProfileId", c => c.Int(nullable: false));
            CreateIndex("dbo.EventProgress", "GameProfile_PersonId");
            AddForeignKey("dbo.EventProgress", "GameProfile_PersonId", "dbo.GameProfile", "PersonId");
        }
    }
}
