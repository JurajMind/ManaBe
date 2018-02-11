namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class seatCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Seat", "Code", c => c.String(maxLength: 5));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Seat", "Code");
        }
    }
}
