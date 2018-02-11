namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlacePublic : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "AllowReservation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Place", "Public", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Place", "Public");
            DropColumn("dbo.Place", "AllowReservation");
        }
    }
}
