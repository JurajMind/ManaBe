namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class cleanToken : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.NotificationToken");
            AlterColumn("dbo.NotificationToken", "Token", c => c.String(nullable: false, maxLength: 255));
            AddPrimaryKey("dbo.NotificationToken", "Token");
            DropColumn("dbo.NotificationToken", "Id");
        }

        public override void Down()
        {
            AddColumn("dbo.NotificationToken", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.NotificationToken");
            AlterColumn("dbo.NotificationToken", "Token", c => c.String());
            AddPrimaryKey("dbo.NotificationToken", "Id");
        }
    }
}
