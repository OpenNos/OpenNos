namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BazaarItem", "MedalUsed", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "IsPackage", c => c.Boolean(nullable: false));
            AddColumn("dbo.BazaarItem", "Amount", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BazaarItem", "Amount");
            DropColumn("dbo.BazaarItem", "IsPackage");
            DropColumn("dbo.BazaarItem", "MedalUsed");
        }
    }
}
