namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class orderExtraPriceGroup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderExtra", "PriceGroupId", c => c.Int());
            CreateIndex("dbo.OrderExtra", "PriceGroupId");
            AddForeignKey("dbo.OrderExtra", "PriceGroupId", "dbo.PriceGroup", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.OrderExtra", "PriceGroupId", "dbo.PriceGroup");
            DropIndex("dbo.OrderExtra", new[] { "PriceGroupId" });
            DropColumn("dbo.OrderExtra", "PriceGroupId");
        }
    }
}
