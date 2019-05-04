namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class features : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SocialMedia",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Url = c.String(),
                        Place_Id = c.Int(),
                        FeatureMixCreator_Id = c.Int(),
                        Brand_Name = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Place", t => t.Place_Id)
                .ForeignKey("dbo.FeatureMixCreator", t => t.FeatureMixCreator_Id)
                .ForeignKey("dbo.Brand", t => t.Brand_Name)
                .Index(t => t.Place_Id)
                .Index(t => t.FeatureMixCreator_Id)
                .Index(t => t.Brand_Name);
            
            CreateTable(
                "dbo.FeatureMixCreator",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        Location = c.String(),
                        LogoPicture = c.String(),
                        PersonId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Friendship",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AId = c.Int(nullable: false),
                        BId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Person", t => t.AId, cascadeDelete: false)
                .ForeignKey("dbo.Person", t => t.BId,cascadeDelete: false)
                .Index(t => t.AId)
                .Index(t => t.BId);
            
            CreateTable(
                "dbo.FeatureMixCreatorFollow",
                c => new
                    {
                        FeatureMixCreatorRefId = c.Int(nullable: false),
                        PersonRefId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.FeatureMixCreatorRefId, t.PersonRefId })
                .ForeignKey("dbo.FeatureMixCreator", t => t.FeatureMixCreatorRefId, cascadeDelete: true)
                .ForeignKey("dbo.Person", t => t.PersonRefId, cascadeDelete: true)
                .Index(t => t.FeatureMixCreatorRefId)
                .Index(t => t.PersonRefId);
            
            AddColumn("dbo.Person", "FeatureMixCreatorId", c => c.Int());
            AddColumn("dbo.Media", "FeatureMixCreator_Id", c => c.Int());
            CreateIndex("dbo.Media", "FeatureMixCreator_Id");
            AddForeignKey("dbo.Media", "FeatureMixCreator_Id", "dbo.FeatureMixCreator", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SocialMedia", "Brand_Name", "dbo.Brand");
            DropForeignKey("dbo.Friendship", "BId", "dbo.Person");
            DropForeignKey("dbo.Friendship", "AId", "dbo.Person");
            DropForeignKey("dbo.SocialMedia", "FeatureMixCreator_Id", "dbo.FeatureMixCreator");
            DropForeignKey("dbo.FeatureMixCreator", "Id", "dbo.Person");
            DropForeignKey("dbo.Media", "FeatureMixCreator_Id", "dbo.FeatureMixCreator");
            DropForeignKey("dbo.FeatureMixCreatorFollow", "PersonRefId", "dbo.Person");
            DropForeignKey("dbo.FeatureMixCreatorFollow", "FeatureMixCreatorRefId", "dbo.FeatureMixCreator");
            DropForeignKey("dbo.SocialMedia", "Place_Id", "dbo.Place");
            DropIndex("dbo.FeatureMixCreatorFollow", new[] { "PersonRefId" });
            DropIndex("dbo.FeatureMixCreatorFollow", new[] { "FeatureMixCreatorRefId" });
            DropIndex("dbo.Friendship", new[] { "BId" });
            DropIndex("dbo.Friendship", new[] { "AId" });
            DropIndex("dbo.FeatureMixCreator", new[] { "Id" });
            DropIndex("dbo.SocialMedia", new[] { "Brand_Name" });
            DropIndex("dbo.SocialMedia", new[] { "FeatureMixCreator_Id" });
            DropIndex("dbo.SocialMedia", new[] { "Place_Id" });
            DropIndex("dbo.Media", new[] { "FeatureMixCreator_Id" });
            DropColumn("dbo.Media", "FeatureMixCreator_Id");
            DropColumn("dbo.Person", "FeatureMixCreatorId");
            DropTable("dbo.FeatureMixCreatorFollow");
            DropTable("dbo.Friendship");
            DropTable("dbo.FeatureMixCreator");
            DropTable("dbo.SocialMedia");
        }
    }
}
