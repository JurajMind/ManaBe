namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class petotest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "Test", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Person", "Test");
        }
    }
}
