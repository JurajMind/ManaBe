namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class brandName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Brand", "DisplayName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Brand", "DisplayName");
        }
    }
}
