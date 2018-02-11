namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Event", "TimeFrame", c => c.Time(precision: 7));
            AddColumn("dbo.EventRecepie", "EventNumber", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.EventRecepie", "EventBool", c => c.Boolean(nullable: false));
            AddColumn("dbo.EventRecepie", "TriggerCount", c => c.Int(nullable: false));
            AddColumn("dbo.EventRecepie", "TriggerCountCompare", c => c.String(maxLength: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventRecepie", "TriggerCountCompare");
            DropColumn("dbo.EventRecepie", "TriggerCount");
            DropColumn("dbo.EventRecepie", "EventBool");
            DropColumn("dbo.EventRecepie", "EventNumber");
            DropColumn("dbo.Event", "TimeFrame");
        }
    }
}
