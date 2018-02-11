namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tobacoStats2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "SmokeDurationTick", c => c.Long(nullable: false));
            AddColumn("dbo.PipeAccesoryStatistics", "SessionDurationTick", c => c.Long(nullable: false));
            DropColumn("dbo.PipeAccesoryStatistics", "SmokeDuration");
            DropColumn("dbo.PipeAccesoryStatistics", "SessionDuration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesoryStatistics", "SessionDuration", c => c.Time(nullable: false, precision: 7));
            AddColumn("dbo.PipeAccesoryStatistics", "SmokeDuration", c => c.Time(nullable: false, precision: 7));
            DropColumn("dbo.PipeAccesoryStatistics", "SessionDurationTick");
            DropColumn("dbo.PipeAccesoryStatistics", "SmokeDurationTick");
        }
    }
}
