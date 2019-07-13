namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mediaSizes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Media", "Sizes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Media", "Sizes");
        }
    }
}
