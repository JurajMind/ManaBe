namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class media : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false),
                        Path = c.String(),
                        Type = c.Int(nullable: false),
                        PipeAccesory_Id = c.Int(),
                        Place_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PipeAccesory", t => t.PipeAccesory_Id)
                .ForeignKey("dbo.Place", t => t.Place_Id)
                .Index(t => t.PipeAccesory_Id)
                .Index(t => t.Place_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Media", "Place_Id", "dbo.Place");
            DropForeignKey("dbo.Media", "PipeAccesory_Id", "dbo.PipeAccesory");
            DropIndex("dbo.Media", new[] { "Place_Id" });
            DropIndex("dbo.Media", new[] { "PipeAccesory_Id" });
            DropTable("dbo.Media");
        }
    }
}
