namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class storedSession : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "StorePath", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.SmokeSession", "StorePath");
        }
    }
}
