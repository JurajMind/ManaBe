namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MultiSeat : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Reservation", "SeatId", "dbo.Seat");
            DropIndex("dbo.Reservation", new[] { "SeatId" });
            CreateTable(
                "dbo.ReservationSeat",
                c => new
                {
                    ReservationRefId = c.Int(nullable: false),
                    SeatRefId = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.ReservationRefId, t.SeatRefId })
                .ForeignKey("dbo.Reservation", t => t.ReservationRefId, cascadeDelete: true)
                .ForeignKey("dbo.Seat", t => t.SeatRefId, cascadeDelete: true)
                .Index(t => t.ReservationRefId)
                .Index(t => t.SeatRefId);

            DropColumn("dbo.Reservation", "SeatId");
        }

        public override void Down()
        {
            AddColumn("dbo.Reservation", "SeatId", c => c.Int());
            DropForeignKey("dbo.ReservationSeat", "SeatRefId", "dbo.Seat");
            DropForeignKey("dbo.ReservationSeat", "ReservationRefId", "dbo.Reservation");
            DropIndex("dbo.ReservationSeat", new[] { "SeatRefId" });
            DropIndex("dbo.ReservationSeat", new[] { "ReservationRefId" });
            DropTable("dbo.ReservationSeat");
            CreateIndex("dbo.Reservation", "SeatId");
            AddForeignKey("dbo.Reservation", "SeatId", "dbo.Seat", "Id");
        }
    }
}
