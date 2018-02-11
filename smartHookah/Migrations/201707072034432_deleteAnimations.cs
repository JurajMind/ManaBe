namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteAnimations : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Animation");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Animation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Code = c.Int(nullable: false),
                        Version = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
