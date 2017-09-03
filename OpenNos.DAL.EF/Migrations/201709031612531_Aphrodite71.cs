namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite71 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RollGeneratedItem", "ItemGeneratedUpgrade", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RollGeneratedItem", "ItemGeneratedUpgrade");
        }
    }
}
