namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite60 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Item", "Flag1", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag2", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag3", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag4", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag5", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag6", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag7", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag8", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "IsMinilandActionable", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "Flag9", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "IsWarehouse", c => c.Boolean(nullable: false));
            DropColumn("dbo.Item", "IsBlocked");
            DropColumn("dbo.Item", "IsHolder");
            DropColumn("dbo.Item", "IsMinilandObject");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Item", "IsMinilandObject", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "IsHolder", c => c.Boolean(nullable: false));
            AddColumn("dbo.Item", "IsBlocked", c => c.Boolean(nullable: false));
            DropColumn("dbo.Item", "IsWarehouse");
            DropColumn("dbo.Item", "Flag9");
            DropColumn("dbo.Item", "IsMinilandActionable");
            DropColumn("dbo.Item", "Flag8");
            DropColumn("dbo.Item", "Flag7");
            DropColumn("dbo.Item", "Flag6");
            DropColumn("dbo.Item", "Flag5");
            DropColumn("dbo.Item", "Flag4");
            DropColumn("dbo.Item", "Flag3");
            DropColumn("dbo.Item", "Flag2");
            DropColumn("dbo.Item", "Flag1");
        }
    }
}
