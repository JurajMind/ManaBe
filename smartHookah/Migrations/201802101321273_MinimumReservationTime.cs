namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MinimumReservationTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "MinimumReservationTime", c => c.Int(nullable: false, defaultValue: 5));
        }

        public override void Down()
        {
            DropColumn("dbo.Place", "MinimumReservationTime");
        }
    }
}
