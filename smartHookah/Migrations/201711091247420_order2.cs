namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class order2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReservationCustomers",
                c => new
                    {
                        ReservationRefId = c.Int(nullable: false),
                        PersonRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ReservationRefId, t.PersonRefId })
                .ForeignKey("dbo.Reservation", t => t.ReservationRefId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonRefId, cascadeDelete: true)
                .Index(t => t.ReservationRefId)
                .Index(t => t.PersonRefId);
            
            AddColumn("dbo.HookahOrder", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Reservation", "Started", c => c.DateTime(nullable: false));
            AddColumn("dbo.Reservation", "End", c => c.DateTime(nullable: false));
            AddColumn("dbo.Reservation", "Text", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReservationCustomers", "PersonRefId", "dbo.Person");
            DropForeignKey("dbo.ReservationCustomers", "ReservationRefId", "dbo.Reservation");
            DropIndex("dbo.ReservationCustomers", new[] { "PersonRefId" });
            DropIndex("dbo.ReservationCustomers", new[] { "ReservationRefId" });
            DropColumn("dbo.Reservation", "Text");
            DropColumn("dbo.Reservation", "End");
            DropColumn("dbo.Reservation", "Started");
            DropColumn("dbo.HookahOrder", "Type");
            DropTable("dbo.ReservationCustomers");
        }
    }
}
