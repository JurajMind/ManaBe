namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class locations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Address", "Location", c => c.Geography());

            //UPDATE [dbo].[Address]
            //SET[Location] = geography::Point([Lat], [Lng], 4326)
            //GO;
        }

        public override void Down()
        {
            DropColumn("dbo.Address", "Location");
        }
    }
}
