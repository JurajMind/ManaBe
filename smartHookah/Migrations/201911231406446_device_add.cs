namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class device_add : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hookah", "Created", c => c.DateTime(nullable: false));
            DropColumn("dbo.DeviceSetting", "Bt");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DeviceSetting", "Bt", c => c.Int(nullable: false));
            DropColumn("dbo.Hookah", "Created");
        }
    }
}
