namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class authClient : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Client",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Secret = c.String(nullable: false),
                    Name = c.String(nullable: false, maxLength: 100),
                    ApplicationType = c.Int(nullable: false),
                    Active = c.Boolean(nullable: false),
                    RefreshTokenLifeTime = c.Int(nullable: false),
                    AllowedOrigin = c.String(maxLength: 100),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.RefreshToken",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Subject = c.String(nullable: false, maxLength: 50),
                    ClientId = c.String(nullable: false, maxLength: 50),
                    IssuedUtc = c.DateTime(nullable: false),
                    ExpiresUtc = c.DateTime(nullable: false),
                    ProtectedTicket = c.String(nullable: false),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.RefreshToken");
            DropTable("dbo.Client");
        }
    }
}
