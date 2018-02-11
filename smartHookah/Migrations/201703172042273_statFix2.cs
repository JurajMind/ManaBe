namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statFix2 : DbMigration
    {
        public override void Up()
        {
          //  DropColumn("dbo.PipeAccesory", "StatisticsId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesory", "StatisticsId", c => c.Int());
        }
    }
}
