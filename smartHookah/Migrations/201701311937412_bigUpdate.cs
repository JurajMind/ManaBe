using smartHookah.Helpers;

namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bigUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PipeAccesory", "Person_Id", "dbo.Person");
            DropForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge");
            DropForeignKey("dbo.Hookah", "PersonId", "dbo.Person");
            DropForeignKey("dbo.PipeAccesory", "Person_Id1", "dbo.Person");
            DropForeignKey("dbo.PipeAccesory", "Person_Id2", "dbo.Person");
            DropIndex("dbo.PipeAccesory", new[] { "Person_Id" });
            DropIndex("dbo.PipeAccesory", new[] { "Person_Id1" });
            DropIndex("dbo.PipeAccesory", new[] { "Person_Id2" });
            DropIndex("dbo.Hookah", new[] { "LoungeId" });
            DropIndex("dbo.Hookah", new[] { "PersonId" });
            CreateTable(
                "dbo.Address",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Street = c.String(),
                        City = c.String(),
                        Number = c.String(),
                        ZIP = c.String(),
                        GoogleMapUri = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OwnPipeAccesories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        PipeAccesoryId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Curent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PipeAccesory", t => t.PipeAccesoryId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId)
                .Index(t => t.PipeAccesoryId);
            
            AddColumn("dbo.Person", "Name", c => c.String());
            AddColumn("dbo.Person", "LoungeId", c => c.Int());
            AddColumn("dbo.Person", "Hookah_Id", c => c.Int());
            AddColumn("dbo.Hookah", "Usage", c => c.Int(nullable: false));
            AddColumn("dbo.Lounge", "Address_Id", c => c.Int());
            CreateIndex("dbo.Person", "LoungeId");
            CreateIndex("dbo.Person", "Hookah_Id");
            CreateIndex("dbo.Lounge", "Address_Id");
            AddForeignKey("dbo.Lounge", "Address_Id", "dbo.Address", "Id");
            AddForeignKey("dbo.Person", "LoungeId", "dbo.Lounge", "Id");
            AddForeignKey("dbo.Person", "Hookah_Id", "dbo.Hookah", "Id");
            Sql("ALTER TABLE [dbo].[PipeAccesory] DROP CONSTRAINT  [FK_dbo.Bowl_dbo.Person_Person_Id]");
            Sql("ALTER TABLE [dbo].[Hookah] DROP CONSTRAINT  [FK_dbo.Hookah_dbo.Person_Person_Id]");
            DropColumn("dbo.PipeAccesory", "Person_Id");
            DropColumn("dbo.PipeAccesory", "Person_Id1");
            DropColumn("dbo.PipeAccesory", "Person_Id2");
            DropColumn("dbo.Hookah", "LoungeId");
            DropColumn("dbo.Hookah", "PersonId");
            DropColumn("dbo.Lounge", "Name");
            DropColumn("dbo.Lounge", "Address_Street");
            DropColumn("dbo.Lounge", "Address_City");
            DropColumn("dbo.Lounge", "Address_Number");
            DropColumn("dbo.Lounge", "Address_ZIP");
            DropColumn("dbo.Lounge", "Address_GoogleMapUri");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Lounge", "Address_GoogleMapUri", c => c.String());
            AddColumn("dbo.Lounge", "Address_ZIP", c => c.String());
            AddColumn("dbo.Lounge", "Address_Number", c => c.String());
            AddColumn("dbo.Lounge", "Address_City", c => c.String());
            AddColumn("dbo.Lounge", "Address_Street", c => c.String());
            AddColumn("dbo.Lounge", "Name", c => c.String());
            AddColumn("dbo.Hookah", "PersonId", c => c.Int());
            AddColumn("dbo.Hookah", "LoungeId", c => c.Int());
            AddColumn("dbo.PipeAccesory", "Person_Id2", c => c.Int());
            AddColumn("dbo.PipeAccesory", "Person_Id1", c => c.Int());
            AddColumn("dbo.PipeAccesory", "Person_Id", c => c.Int());
            DropForeignKey("dbo.Person", "Hookah_Id", "dbo.Hookah");
            DropForeignKey("dbo.OwnPipeAccesories", "PersonId", "dbo.Person");
            DropForeignKey("dbo.OwnPipeAccesories", "PipeAccesoryId", "dbo.PipeAccesory");
            DropForeignKey("dbo.Person", "LoungeId", "dbo.Lounge");
            DropForeignKey("dbo.Lounge", "Address_Id", "dbo.Address");
            DropIndex("dbo.OwnPipeAccesories", new[] { "PipeAccesoryId" });
            DropIndex("dbo.OwnPipeAccesories", new[] { "PersonId" });
            DropIndex("dbo.Lounge", new[] { "Address_Id" });
            DropIndex("dbo.Person", new[] { "Hookah_Id" });
            DropIndex("dbo.Person", new[] { "LoungeId" });
            DropColumn("dbo.Lounge", "Address_Id");
            DropColumn("dbo.Hookah", "Usage");
            DropColumn("dbo.Person", "Hookah_Id");
            DropColumn("dbo.Person", "LoungeId");
            DropColumn("dbo.Person", "Name");
            DropTable("dbo.OwnPipeAccesories");
            DropTable("dbo.Address");
            CreateIndex("dbo.Hookah", "PersonId");
            CreateIndex("dbo.Hookah", "LoungeId");
            CreateIndex("dbo.PipeAccesory", "Person_Id2");
            CreateIndex("dbo.PipeAccesory", "Person_Id1");
            CreateIndex("dbo.PipeAccesory", "Person_Id");
            AddForeignKey("dbo.PipeAccesory", "Person_Id2", "dbo.Person", "Id");
            AddForeignKey("dbo.PipeAccesory", "Person_Id1", "dbo.Person", "Id");
            AddForeignKey("dbo.Hookah", "PersonId", "dbo.Person", "Id");
            AddForeignKey("dbo.Hookah", "LoungeId", "dbo.Lounge", "Id");
            AddForeignKey("dbo.PipeAccesory", "Person_Id", "dbo.Person", "Id");
        }
    }
}
