namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ownDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OwnPipeAccesories", "CreatedDate", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
            AddColumn("dbo.OwnPipeAccesories", "DeleteDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OwnPipeAccesories", "DeleteDate");
            DropColumn("dbo.OwnPipeAccesories", "CreatedDate");
        }
    }
}
