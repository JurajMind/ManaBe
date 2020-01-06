namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mixorigin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "OriginMixId", c => c.Int());
            CreateIndex("dbo.PipeAccesory", "OriginMixId");
            AddForeignKey("dbo.PipeAccesory", "OriginMixId", "dbo.PipeAccesory", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PipeAccesory", "OriginMixId", "dbo.PipeAccesory");
            DropIndex("dbo.PipeAccesory", new[] { "OriginMixId" });
            DropColumn("dbo.PipeAccesory", "OriginMixId");
        }
    }
}
