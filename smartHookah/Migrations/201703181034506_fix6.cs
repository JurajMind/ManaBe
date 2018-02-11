namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix6 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PipeAccesoryStatistics", new[] { "PipeAccesoryId" });
            DropPrimaryKey("dbo.PipeAccesoryStatistics");
            AlterColumn("dbo.PipeAccesoryStatistics", "PipeAccesoryId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PipeAccesoryStatistics", "PipeAccesoryId");
            CreateIndex("dbo.PipeAccesoryStatistics", "PipeAccesoryId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PipeAccesoryStatistics", new[] { "PipeAccesoryId" });
            DropPrimaryKey("dbo.PipeAccesoryStatistics");
            AlterColumn("dbo.PipeAccesoryStatistics", "PipeAccesoryId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PipeAccesoryStatistics", "PipeAccesoryId");
            CreateIndex("dbo.PipeAccesoryStatistics", "PipeAccesoryId");
        }
    }
}
