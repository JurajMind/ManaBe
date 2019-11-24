namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class device_setting_clean : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DeviceSetting", "DevicePresetId");
        }

        public override void Down()
        {
            AddColumn("dbo.DeviceSetting", "DevicePresetId", c => c.Int());
        }
    }
}
