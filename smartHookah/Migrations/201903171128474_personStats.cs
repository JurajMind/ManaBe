namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class personStats : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PersonStatistic",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TotalPufCount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgPufCount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AvgSessionDurationTick = c.Long(nullable: false),
                        TotalDurationTick = c.Long(nullable: false),
                        AvgSmokeTimeTick = c.Long(nullable: false),
                        TotalSmokeTimeTick = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PersonStatistic", "Id", "dbo.Person");
            DropIndex("dbo.PersonStatistic", new[] { "Id" });
            DropTable("dbo.PersonStatistic");
        }
    }
}
