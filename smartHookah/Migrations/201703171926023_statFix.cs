namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statFix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PipeAccesory", "StatisticsId", "dbo.PipeAccesoryStatistics");
            DropIndex("dbo.PipeAccesory", new[] { "StatisticsId" });
            //DropColumn("dbo.PipeAccesoryStatistics", "StatisticsId");
            //RenameColumn(table: "dbo.PipeAccesoryStatistics", name: "StatisticsId", newName: "Id");
            DropPrimaryKey("dbo.PipeAccesoryStatistics");
            AlterColumn("dbo.PipeAccesoryStatistics", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.PipeAccesoryStatistics", "Id");
            CreateIndex("dbo.PipeAccesoryStatistics", "Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PipeAccesoryStatistics", new[] { "Id" });
            DropPrimaryKey("dbo.PipeAccesoryStatistics");
            AlterColumn("dbo.PipeAccesoryStatistics", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.PipeAccesoryStatistics", "Id");
            RenameColumn(table: "dbo.PipeAccesoryStatistics", name: "Id", newName: "StatisticsId");
            AddColumn("dbo.PipeAccesoryStatistics", "Id", c => c.Int(nullable: false, identity: true));
            CreateIndex("dbo.PipeAccesory", "StatisticsId");
            AddForeignKey("dbo.PipeAccesory", "StatisticsId", "dbo.PipeAccesoryStatistics", "Id");
        }
    }
}
