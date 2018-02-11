namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix4 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PipeAccesoryStatistics", name: "Id", newName: "PipeAccesoryId");
            RenameIndex(table: "dbo.PipeAccesoryStatistics", name: "IX_Id", newName: "IX_PipeAccesoryId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.PipeAccesoryStatistics", name: "IX_PipeAccesoryId", newName: "IX_Id");
            RenameColumn(table: "dbo.PipeAccesoryStatistics", name: "PipeAccesoryId", newName: "Id");
        }
    }
}
