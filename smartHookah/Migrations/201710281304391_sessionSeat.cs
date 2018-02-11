namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sessionSeat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "SeatId", c => c.Int());
            CreateIndex("dbo.SmokeSession", "SeatId");
            AddForeignKey("dbo.SmokeSession", "SeatId", "dbo.Seat", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmokeSession", "SeatId", "dbo.Seat");
            DropIndex("dbo.SmokeSession", new[] { "SeatId" });
            DropColumn("dbo.SmokeSession", "SeatId");
        }
    }
}
