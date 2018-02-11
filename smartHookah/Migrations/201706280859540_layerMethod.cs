namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class layerMethod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "LayerMethod", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.PipeAccesory", "LayerMethod");
        }
    }
}
