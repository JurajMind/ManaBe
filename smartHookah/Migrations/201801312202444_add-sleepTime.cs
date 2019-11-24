namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class addsleepTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hookah", "AutoSessionEndTime", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Hookah", "AutoSessionEndTime");
        }
    }
}
