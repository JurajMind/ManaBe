namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class order5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservation", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reservation", "Name");
        }
    }
}
