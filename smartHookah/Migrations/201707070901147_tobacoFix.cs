namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tobacoFix : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.PipeAccesory", "Type");
            DropColumn("dbo.PipeAccesory", "MixName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PipeAccesory", "MixName", c => c.String());
            AddColumn("dbo.PipeAccesory", "Type", c => c.String());
        }
    }
}
