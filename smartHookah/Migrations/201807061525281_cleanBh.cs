namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class cleanBh : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BusinessHours", "LoungeId");
        }

        public override void Down()
        {
            AddColumn("dbo.BusinessHours", "LoungeId", c => c.Int(nullable: false));
        }
    }
}
