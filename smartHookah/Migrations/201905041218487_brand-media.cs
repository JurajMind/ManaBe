namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class brandmedia : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Media", "Brand_Name", c => c.String(maxLength: 128));
            CreateIndex("dbo.Media", "Brand_Name");
            AddForeignKey("dbo.Media", "Brand_Name", "dbo.Brand", "Name");
        }

        public override void Down()
        {
            DropForeignKey("dbo.Media", "Brand_Name", "dbo.Brand");
            DropIndex("dbo.Media", new[] { "Brand_Name" });
            DropColumn("dbo.Media", "Brand_Name");
        }
    }
}
