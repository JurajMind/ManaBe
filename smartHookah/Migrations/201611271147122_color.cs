namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class color : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahSetting", "Color_Hue", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Saturation", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Value", c => c.Byte(nullable: false));
            DropColumn("dbo.HookahSetting", "Color_Red");
            DropColumn("dbo.HookahSetting", "Color_Green");
            DropColumn("dbo.HookahSetting", "Color_Blue");
        }
        
        public override void Down()
        {
            AddColumn("dbo.HookahSetting", "Color_Blue", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Green", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Red", c => c.Byte(nullable: false));
            DropColumn("dbo.HookahSetting", "Color_Value");
            DropColumn("dbo.HookahSetting", "Color_Saturation");
            DropColumn("dbo.HookahSetting", "Color_Hue");
        }
    }
}
