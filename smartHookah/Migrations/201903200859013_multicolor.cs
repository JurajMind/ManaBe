namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class multicolor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeviceSetting", "PufColor_Hue", c => c.Byte(nullable: false));
            AddColumn("dbo.DeviceSetting", "PufColor_Saturation", c => c.Byte(nullable: false));
            AddColumn("dbo.DeviceSetting", "PufColor_Value", c => c.Byte(nullable: false));
            AddColumn("dbo.DeviceSetting", "BlowColor_Hue", c => c.Byte(nullable: false));
            AddColumn("dbo.DeviceSetting", "BlowColor_Saturation", c => c.Byte(nullable: false));
            AddColumn("dbo.DeviceSetting", "BlowColor_Value", c => c.Byte(nullable: false));
            AddColumn("dbo.Reservation", "Late", c => c.Int());
            AddColumn("dbo.Reservation", "Src", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Reservation", "Src");
            DropColumn("dbo.Reservation", "Late");
            DropColumn("dbo.DeviceSetting", "BlowColor_Value");
            DropColumn("dbo.DeviceSetting", "BlowColor_Saturation");
            DropColumn("dbo.DeviceSetting", "BlowColor_Hue");
            DropColumn("dbo.DeviceSetting", "PufColor_Value");
            DropColumn("dbo.DeviceSetting", "PufColor_Saturation");
            DropColumn("dbo.DeviceSetting", "PufColor_Hue");
        }
    }
}
