namespace smartHookah.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class accPictures : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PipeAccesory", "Picture", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PipeAccesory", "Picture");
        }
    }
}
