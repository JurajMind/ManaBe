namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class person_refactor1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DevicePreset", "DeviceSettingId", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.DevicePreset", "DeviceSettingId");
        }
    }
}
