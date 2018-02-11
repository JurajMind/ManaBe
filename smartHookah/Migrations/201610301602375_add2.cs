namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add2 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Hookah", name: "Person_Id", newName: "PersonId");
            RenameIndex(table: "dbo.Hookah", name: "IX_Person_Id", newName: "IX_PersonId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Hookah", name: "IX_PersonId", newName: "IX_Person_Id");
            RenameColumn(table: "dbo.Hookah", name: "PersonId", newName: "Person_Id");
        }
    }
}
