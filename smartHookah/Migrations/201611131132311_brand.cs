namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class brand : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PipeAccesory", name: "Brand_Name", newName: "BrandName");
            RenameIndex(table: "dbo.PipeAccesory", name: "IX_Brand_Name", newName: "IX_BrandName");
            DropColumn("dbo.PipeAccesory", "BrandId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesory", "BrandId", c => c.String());
            RenameIndex(table: "dbo.PipeAccesory", name: "IX_BrandName", newName: "IX_Brand_Name");
            RenameColumn(table: "dbo.PipeAccesory", name: "BrandName", newName: "Brand_Name");
        }
    }
}
