namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GamePerson",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        PersonId = c.Int(nullable: false),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.Id)
                .Index(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GamePerson", "Id", "dbo.Person");
            DropIndex("dbo.GamePerson", new[] { "Id" });
            DropTable("dbo.GamePerson");
        }
    }
}
