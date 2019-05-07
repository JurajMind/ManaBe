namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reservationlatefix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "State", c => c.Int(nullable: false));
            AddColumn("dbo.Reservation", "LateDuration", c => c.Int());
            DropColumn("dbo.Place", "Instagram");
            DropColumn("dbo.Reservation", "Late");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Reservation", "Late", c => c.Int());
            AddColumn("dbo.Place", "Instagram", c => c.String());
            DropColumn("dbo.Reservation", "LateDuration");
            DropColumn("dbo.Place", "State");
        }
    }
}
