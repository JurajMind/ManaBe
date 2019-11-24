namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Device_preset : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HookahPersonSetting", "PersonId", "dbo.Person");
            DropIndex("dbo.HookahPersonSetting", new[] { "PersonId" });
            AlterColumn("dbo.HookahPersonSetting", "PersonId", c => c.Int());
            CreateIndex("dbo.HookahPersonSetting", "PersonId");
            AddForeignKey("dbo.HookahPersonSetting", "PersonId", "dbo.Person", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.HookahPersonSetting", "PersonId", "dbo.Person");
            DropIndex("dbo.HookahPersonSetting", new[] { "PersonId" });
            AlterColumn("dbo.HookahPersonSetting", "PersonId", c => c.Int(nullable: false));
            CreateIndex("dbo.HookahPersonSetting", "PersonId");
            AddForeignKey("dbo.HookahPersonSetting", "PersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
    }
}
