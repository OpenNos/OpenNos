namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite40 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Mate", "Skin", c => c.Short(nullable: false));
            DropColumn("dbo.Mate", "HasSkin");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Mate", "HasSkin", c => c.Boolean(nullable: false));
            DropColumn("dbo.Mate", "Skin");
        }
    }
}
