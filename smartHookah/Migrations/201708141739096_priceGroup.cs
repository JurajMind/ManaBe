namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class priceGroup : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .Index(t => t.PlaceId);
            
            CreateTable(
                "dbo.OrderExtra",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PriceId = c.Int(),
                        PlaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.PlaceId, cascadeDelete: true)
                .ForeignKey("dbo.PriceGroup", t => t.PriceId)
                .Index(t => t.PriceId)
                .Index(t => t.PlaceId);
            
            AddColumn("dbo.HookahOrder", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2,defaultValueSql:"0"));
            AddColumn("dbo.HookahOrder", "Currency", c => c.String(maxLength: 3,defaultValueSql:"'CZK'"));
            AddColumn("dbo.OwnPipeAccesories", "PriceId", c => c.Int());
            CreateIndex("dbo.OwnPipeAccesories", "PriceId");
            AddForeignKey("dbo.OwnPipeAccesories", "PriceId", "dbo.PriceGroup", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OwnPipeAccesories", "PriceId", "dbo.PriceGroup");
            DropForeignKey("dbo.PriceGroup", "PlaceId", "dbo.Place");
            DropForeignKey("dbo.OrderExtra", "PriceId", "dbo.PriceGroup");
            DropForeignKey("dbo.OrderExtra", "PlaceId", "dbo.Place");
            DropIndex("dbo.OwnPipeAccesories", new[] { "PriceId" });
            DropIndex("dbo.OrderExtra", new[] { "PlaceId" });
            DropIndex("dbo.OrderExtra", new[] { "PriceId" });
            DropIndex("dbo.PriceGroup", new[] { "PlaceId" });
            DropColumn("dbo.OwnPipeAccesories", "PriceId");
            DropColumn("dbo.HookahOrder", "Currency");
            DropColumn("dbo.HookahOrder", "Price");
            DropTable("dbo.OrderExtra");
            DropTable("dbo.PriceGroup");
        }
    }
}
