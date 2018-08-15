namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class coalHms : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Brand", "Coal", c => c.Boolean(nullable: false));
            AddColumn("dbo.Brand", "HeatManagment", c => c.Boolean(nullable: false));
            AddColumn("dbo.SmokeSessionMetaData", "CoalId", c => c.Int());
            AddColumn("dbo.SmokeSessionMetaData", "HeatManagementId", c => c.Int());
            CreateIndex("dbo.SmokeSessionMetaData", "CoalId");
            CreateIndex("dbo.SmokeSessionMetaData", "HeatManagementId");
            AddForeignKey("dbo.SmokeSessionMetaData", "CoalId", "dbo.PipeAccesory", "Id");
            AddForeignKey("dbo.SmokeSessionMetaData", "HeatManagementId", "dbo.PipeAccesory", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmokeSessionMetaData", "HeatManagementId", "dbo.PipeAccesory");
            DropForeignKey("dbo.SmokeSessionMetaData", "CoalId", "dbo.PipeAccesory");
            DropIndex("dbo.SmokeSessionMetaData", new[] { "HeatManagementId" });
            DropIndex("dbo.SmokeSessionMetaData", new[] { "CoalId" });
            DropColumn("dbo.SmokeSessionMetaData", "HeatManagementId");
            DropColumn("dbo.SmokeSessionMetaData", "CoalId");
            DropColumn("dbo.Brand", "HeatManagment");
            DropColumn("dbo.Brand", "Coal");
        }
    }
}
