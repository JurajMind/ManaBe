namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class review : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PipeAccesoryStatistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Used = c.Int(nullable: false),
                        Weight = c.Int(nullable: false),
                        SmokeDuration = c.Time(nullable: false, precision: 7),
                        PufCount = c.Int(nullable: false),
                        BlowCount = c.Int(nullable: false),
                        SessionDuration = c.Time(nullable: false, precision: 7),
                        PackType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TobaccoReview",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AuthorId = c.Int(),
                        PublishDate = c.DateTime(nullable: false),
                        Quality = c.Int(nullable: false),
                        Taste = c.Int(nullable: false),
                        Smoke = c.Int(nullable: false),
                        Overall = c.Int(nullable: false),
                        ReviewedTobaccoId = c.Int(nullable: false),
                        SmokeSessionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AuthorId)
                .ForeignKey("dbo.PipeAccesory", t => t.ReviewedTobaccoId, cascadeDelete: true)
                .ForeignKey("dbo.SmokeSession", t => t.SmokeSessionId)
                .Index(t => t.AuthorId)
                .Index(t => t.ReviewedTobaccoId)
                .Index(t => t.SmokeSessionId);
            
            AddColumn("dbo.PipeAccesory", "StatisticsId", c => c.Int());
            CreateIndex("dbo.PipeAccesory", "StatisticsId");
            AddForeignKey("dbo.PipeAccesory", "StatisticsId", "dbo.PipeAccesoryStatistics", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TobaccoReview", "SmokeSessionId", "dbo.SmokeSession");
            DropForeignKey("dbo.TobaccoReview", "ReviewedTobaccoId", "dbo.PipeAccesory");
            DropForeignKey("dbo.TobaccoReview", "AuthorId", "dbo.Person");
            DropForeignKey("dbo.PipeAccesory", "StatisticsId", "dbo.PipeAccesoryStatistics");
            DropIndex("dbo.TobaccoReview", new[] { "SmokeSessionId" });
            DropIndex("dbo.TobaccoReview", new[] { "ReviewedTobaccoId" });
            DropIndex("dbo.TobaccoReview", new[] { "AuthorId" });
            DropIndex("dbo.PipeAccesory", new[] { "StatisticsId" });
            DropColumn("dbo.PipeAccesory", "StatisticsId");
            DropTable("dbo.TobaccoReview");
            DropTable("dbo.PipeAccesoryStatistics");
        }
    }
}
