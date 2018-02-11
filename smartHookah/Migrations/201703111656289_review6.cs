namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class review6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmokeSession", "Review_Id", c => c.Int());
            CreateIndex("dbo.SmokeSession", "Review_Id");
            AddForeignKey("dbo.SmokeSession", "Review_Id", "dbo.TobaccoReview", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SmokeSession", "Review_Id", "dbo.TobaccoReview");
            DropIndex("dbo.SmokeSession", new[] { "Review_Id" });
            DropColumn("dbo.SmokeSession", "Review_Id");
        }
    }
}
