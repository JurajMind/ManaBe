namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PriceFix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrderExtra", "PriceId", "dbo.PriceGroup");
            DropForeignKey("dbo.PriceGroup", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.OwnPipeAccesories", "PriceId", "dbo.PriceGroup");
            DropIndex("dbo.PriceGroup", new[] { "PlaceId" });
            DropIndex("dbo.OrderExtra", new[] { "PriceId" });
            DropIndex("dbo.OwnPipeAccesories", new[] { "PriceId" });
            AddColumn("dbo.OrderExtra", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OrderExtra", "Currency", c => c.String(maxLength: 3));
            AddColumn("dbo.OwnPipeAccesories", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.OwnPipeAccesories", "Currency", c => c.String(maxLength: 3));
            DropTable("dbo.PriceGroup");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PriceGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(maxLength: 3),
                        PlaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.OwnPipeAccesories", "Currency");
            DropColumn("dbo.OwnPipeAccesories", "Price");
            DropColumn("dbo.OrderExtra", "Currency");
            DropColumn("dbo.OrderExtra", "Price");
            CreateIndex("dbo.OwnPipeAccesories", "PriceId");
            CreateIndex("dbo.OrderExtra", "PriceId");
            CreateIndex("dbo.PriceGroup", "PlaceId");
            AddForeignKey("dbo.OwnPipeAccesories", "PriceId", "dbo.PriceGroup", "Id");
            AddForeignKey("dbo.PriceGroup", "PlaceId", "dbo.Place", "Id", cascadeDelete: true);
            AddForeignKey("dbo.OrderExtra", "PriceId", "dbo.PriceGroup", "Id");
        }
    }
}
