namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class autoSleepSetting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hookah", "AutoSleep", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hookah", "AutoSleep");
        }
    }
}
