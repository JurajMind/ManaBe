namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pricePlace : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Place", "BaseHookahPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Place", "Currency", c => c.String(maxLength: 3));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Place", "Currency");
            DropColumn("dbo.Place", "BaseHookahPrice");
        }
    }
}
