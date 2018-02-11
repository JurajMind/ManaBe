namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Event", "TimeFrameTicks", c => c.Long());
            DropColumn("dbo.Event", "TimeFrame");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Event", "TimeFrame", c => c.Time(precision: 7));
            DropColumn("dbo.Event", "TimeFrameTicks");
        }
    }
}
