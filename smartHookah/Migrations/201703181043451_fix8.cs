namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fix8 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PipeAccesoryStatistics", "PipeAccesoryId", "dbo.PipeAccesory");
            DropIndex("dbo.PipeAccesoryStatistics", new[] { "PipeAccesoryId" });
            DropTable("dbo.PipeAccesoryStatistics");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PipeAccesoryStatistics",
                c => new
                    {
                        PipeAccesoryId = c.Int(nullable: false, identity: true),
                        Used = c.Int(nullable: false),
                        Weight = c.Double(nullable: false),
                        SmokeDurationTick = c.Long(nullable: false),
                        PufCount = c.Double(nullable: false),
                        BlowCount = c.Double(nullable: false),
                        SessionDurationTick = c.Long(nullable: false),
                        PackType = c.Int(nullable: false),
                        Quality = c.Double(nullable: false),
                        Taste = c.Double(nullable: false),
                        Smoke = c.Double(nullable: false),
                        Overall = c.Double(nullable: false),
                        SmokeTimePercentil = c.Double(nullable: false),
                        SessionTimePercentil = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.PipeAccesoryId);
            
            CreateIndex("dbo.PipeAccesoryStatistics", "PipeAccesoryId");
            AddForeignKey("dbo.PipeAccesoryStatistics", "PipeAccesoryId", "dbo.PipeAccesory", "Id");
        }
    }
}
