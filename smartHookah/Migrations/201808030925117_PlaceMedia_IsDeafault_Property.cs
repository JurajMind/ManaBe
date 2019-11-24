namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class PlaceMedia_IsDeafault_Property : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Media", "IsDefault", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Media", "IsDefault");
        }
    }
}
