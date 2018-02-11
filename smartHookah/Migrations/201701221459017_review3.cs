namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class review3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TobaccoReview", "Text", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TobaccoReview", "Text");
        }
    }
}
