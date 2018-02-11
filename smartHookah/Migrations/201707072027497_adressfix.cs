namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adressfix : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Lounge", "Address_Id", "dbo.Address");
            DropIndex("dbo.Lounge", new[] { "Address_Id" });
            RenameColumn(table: "dbo.Lounge", name: "Address_Id", newName: "AddressId");
            AlterColumn("dbo.Lounge", "AddressId", c => c.Int(nullable: false));
            CreateIndex("dbo.Lounge", "AddressId");
            AddForeignKey("dbo.Lounge", "AddressId", "dbo.Address", "Id", cascadeDelete: true);
            DropColumn("dbo.Lounge", "AdressId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Lounge", "AdressId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Lounge", "AddressId", "dbo.Address");
            DropIndex("dbo.Lounge", new[] { "AddressId" });
            AlterColumn("dbo.Lounge", "AddressId", c => c.Int());
            RenameColumn(table: "dbo.Lounge", name: "AddressId", newName: "Address_Id");
            CreateIndex("dbo.Lounge", "Address_Id");
            AddForeignKey("dbo.Lounge", "Address_Id", "dbo.Address", "Id");
        }
    }
}
