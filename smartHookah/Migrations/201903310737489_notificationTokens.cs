namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notificationTokens : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationToken",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        Token = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationToken", "PersonId", "dbo.Person");
            DropIndex("dbo.NotificationToken", new[] { "PersonId" });
            DropTable("dbo.NotificationToken");
        }
    }
}
