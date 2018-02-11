namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class alternativeName1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OwnPipeAccesories", "AlternativeName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OwnPipeAccesories", "AlternativeName");
        }
    }
}
