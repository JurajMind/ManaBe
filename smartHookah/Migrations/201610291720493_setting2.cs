namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class setting2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HookahSetting", "PufAnimation", c => c.Int(nullable: false));
            AddColumn("dbo.HookahSetting", "BlowAnimation", c => c.Int(nullable: false));
            AddColumn("dbo.HookahSetting", "IdleAnimation", c => c.Int(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Red", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Green", c => c.Byte(nullable: false));
            AddColumn("dbo.HookahSetting", "Color_Blue", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.HookahSetting", "Color_Blue");
            DropColumn("dbo.HookahSetting", "Color_Green");
            DropColumn("dbo.HookahSetting", "Color_Red");
            DropColumn("dbo.HookahSetting", "IdleAnimation");
            DropColumn("dbo.HookahSetting", "BlowAnimation");
            DropColumn("dbo.HookahSetting", "PufAnimation");
        }
    }
}
