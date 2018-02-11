namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class order4 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SmokeSession", "SeatId", "dbo.Seat");
            DropIndex("dbo.SmokeSession", new[] { "SeatId" });
            DropColumn("dbo.SmokeSession", "SeatId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SmokeSession", "SeatId", c => c.Int());
            CreateIndex("dbo.SmokeSession", "SeatId");
            AddForeignKey("dbo.SmokeSession", "SeatId", "dbo.Seat", "Id");
        }
    }
}
