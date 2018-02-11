namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class myTobacco : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Person", "MyGear", c => c.Boolean(nullable: false,defaultValueSql:"1"));
            AddColumn("dbo.Person", "MyTobacco", c => c.Boolean(nullable: false, defaultValueSql: "1"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Person", "MyTobacco");
            DropColumn("dbo.Person", "MyGear");
        }
    }
}
