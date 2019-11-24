namespace smartHookah.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class priceGroup1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Franchise",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    Uril = c.String(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.PriceGroup",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    PlaceId = c.Int(nullable: false),
                    Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .Index(t => t.PlaceId);

            AddColumn("dbo.Place", "FranchiseId", c => c.Int());
            AddColumn("dbo.OwnPipeAccesories", "PriceGroupId", c => c.Int());
            CreateIndex("dbo.Place", "FranchiseId");
            CreateIndex("dbo.OwnPipeAccesories", "PriceGroupId");
            AddForeignKey("dbo.Place", "FranchiseId", "dbo.Franchise", "Id");
            AddForeignKey("dbo.OwnPipeAccesories", "PriceGroupId", "dbo.PriceGroup", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.PriceGroup", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.OwnPipeAccesories", "PriceGroupId", "dbo.PriceGroup");
            DropForeignKey("dbo.Place", "FranchiseId", "dbo.Franchise");
            DropIndex("dbo.OwnPipeAccesories", new[] { "PriceGroupId" });
            DropIndex("dbo.PriceGroup", new[] { "PlaceId" });
            DropIndex("dbo.Place", new[] { "FranchiseId" });
            DropColumn("dbo.OwnPipeAccesories", "PriceGroupId");
            DropColumn("dbo.Place", "FranchiseId");
            DropTable("dbo.PriceGroup");
            DropTable("dbo.Franchise");
        }
    }
}
