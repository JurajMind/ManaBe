namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gdpr : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PersonPipeAccesory",
                c => new
                    {
                        Person_Id = c.Int(nullable: false),
                        PipeAccesory_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Person_Id, t.PipeAccesory_Id })
                .ForeignKey("dbo.Person", t => t.Person_Id, cascadeDelete: true)
                .ForeignKey("dbo.PipeAccesory", t => t.PipeAccesory_Id, cascadeDelete: true)
                .Index(t => t.Person_Id)
                .Index(t => t.PipeAccesory_Id);
            
            AddColumn("dbo.Person", "Gdpr", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PersonPipeAccesory", "PipeAccesory_Id", "dbo.PipeAccesory");
            DropForeignKey("dbo.PersonPipeAccesory", "Person_Id", "dbo.Person");
            DropIndex("dbo.PersonPipeAccesory", new[] { "PipeAccesory_Id" });
            DropIndex("dbo.PersonPipeAccesory", new[] { "Person_Id" });
            DropColumn("dbo.Person", "Gdpr");
            DropTable("dbo.PersonPipeAccesory");
        }
    }
}
