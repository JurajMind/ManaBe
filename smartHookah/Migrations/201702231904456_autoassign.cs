namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class autoassign : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "AutoAssign", c => c.Boolean(nullable: false,defaultValue:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Person", "AutoAssign");
        }
    }
}
