namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seat2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahOrder", "SeatId", c => c.Int());
            CreateIndex("dbo.HookahOrder", "SeatId");
            AddForeignKey("dbo.HookahOrder", "SeatId", "dbo.Seat", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HookahOrder", "SeatId", "dbo.Seat");
            DropIndex("dbo.HookahOrder", new[] { "SeatId" });
            DropColumn("dbo.HookahOrder", "SeatId");
        }
    }
}
