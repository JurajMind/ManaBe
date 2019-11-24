namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class GroupPriceFix : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.OwnPipeAccesories", name: "PriceGroupId", newName: "PriceGroup_Id");
            RenameIndex(table: "dbo.OwnPipeAccesories", name: "IX_PriceGroupId", newName: "IX_PriceGroup_Id");
            CreateTable(
                "dbo.PriceGroupPrice",
                c => new
                {
                    OwnPipeAccesoriesId = c.Int(nullable: false),
                    PriceGroupId = c.Int(nullable: false),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => new { t.OwnPipeAccesoriesId, t.PriceGroupId })
                .ForeignKey("dbo.PriceGroup", t => t.PriceGroupId, cascadeDelete: true)
                .ForeignKey("dbo.OwnPipeAccesories", t => t.OwnPipeAccesoriesId, cascadeDelete: true)
                .Index(t => t.OwnPipeAccesoriesId)
                .Index(t => t.PriceGroupId);

            AlterColumn("dbo.OwnPipeAccesories", "Currency", c => c.String());
            DropColumn("dbo.OwnPipeAccesories", "PriceId");
            DropColumn("dbo.OwnPipeAccesories", "Price");
        }

        public override void Down()
        {
            AddColumn("dbo.OwnPipeAccesories", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OwnPipeAccesories", "PriceId", c => c.Int());
            DropForeignKey("dbo.PriceGroupPrice", "OwnPipeAccesoriesId", "dbo.OwnPipeAccesories");
            DropForeignKey("dbo.PriceGroupPrice", "PriceGroupId", "dbo.PriceGroup");
            DropIndex("dbo.PriceGroupPrice", new[] { "PriceGroupId" });
            DropIndex("dbo.PriceGroupPrice", new[] { "OwnPipeAccesoriesId" });
            AlterColumn("dbo.OwnPipeAccesories", "Currency", c => c.String(maxLength: 3));
            DropTable("dbo.PriceGroupPrice");
            RenameIndex(table: "dbo.OwnPipeAccesories", name: "IX_PriceGroup_Id", newName: "IX_PriceGroupId");
            RenameColumn(table: "dbo.OwnPipeAccesories", name: "PriceGroup_Id", newName: "PriceGroupId");
        }
    }
}
