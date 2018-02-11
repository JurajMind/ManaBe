namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reservation3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Reservation", "Started", c => c.DateTime());
            AlterColumn("dbo.Reservation", "End", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Reservation", "End", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Reservation", "Started", c => c.DateTime(nullable: false));
        }
    }
}
