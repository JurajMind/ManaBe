namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placeimprovments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Address", "AddressId", c => c.Int(nullable: false));
            AddColumn("dbo.Address", "Country", c => c.String());
            AddColumn("dbo.Place", "Instagram", c => c.String());
            AddColumn("dbo.Place", "Url", c => c.String());
            AddColumn("dbo.Place", "HaveReservation", c => c.Boolean(nullable: false));
            AddColumn("dbo.Place", "HaveMenu", c => c.Boolean(nullable: false));
            AddColumn("dbo.Place", "HaveOrders", c => c.Boolean(nullable: false));
            AddColumn("dbo.Place", "HaveMana", c => c.Boolean(nullable: false));
            AddColumn("dbo.Place", "Rating", c => c.Int(nullable: false));
            AddColumn("dbo.Place", "Src", c => c.Int(nullable: false));
            DropColumn("dbo.Place", "AllowReservation");
            DropColumn("dbo.PlaceFlag", "DisplayName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PlaceFlag", "DisplayName", c => c.String());
            AddColumn("dbo.Place", "AllowReservation", c => c.Boolean(nullable: false));
            DropColumn("dbo.Place", "Src");
            DropColumn("dbo.Place", "Rating");
            DropColumn("dbo.Place", "HaveMana");
            DropColumn("dbo.Place", "HaveOrders");
            DropColumn("dbo.Place", "HaveMenu");
            DropColumn("dbo.Place", "HaveReservation");
            DropColumn("dbo.Place", "Url");
            DropColumn("dbo.Place", "Instagram");
            DropColumn("dbo.Address", "Country");
            DropColumn("dbo.Address", "AddressId");
        }
    }
}
