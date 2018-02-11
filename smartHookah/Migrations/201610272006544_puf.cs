namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class puf : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DbPuf", "Presure", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DbPuf", "Presure");
        }
    }
}
