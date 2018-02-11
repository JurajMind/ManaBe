namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class game4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameProfile", "Picture_Id", c => c.Int());
            CreateIndex("dbo.GameProfile", "Picture_Id");
            AddForeignKey("dbo.GameProfile", "Picture_Id", "dbo.Reward", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GameProfile", "Picture_Id", "dbo.Reward");
            DropIndex("dbo.GameProfile", new[] { "Picture_Id" });
            DropColumn("dbo.GameProfile", "Picture_Id");
        }
    }
}
