namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDisplayName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ApplicationUser", "DisplayName", c => c.String(maxLength: 50,defaultValueSql:"User"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ApplicationUser", "DisplayName");
        }
    }
}
