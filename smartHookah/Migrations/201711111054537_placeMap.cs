namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placeMap : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "Map", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Place", "Map");
        }
    }
}
